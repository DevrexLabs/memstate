using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Memstate.Logging;

namespace Memstate
{
    public class Engine<TModel> where TModel : class
    {
        public EngineState State { get; set; } = EngineState.NotStarted;

        private readonly ILog _logger;

        private readonly Kernel _kernel;

        private readonly EngineSettings _settings;

        private readonly IJournalWriter _journalWriter;

        private Task _readerTask;

        /// <summary>
        /// Commands that have been sent to the journal but not yet received 
        /// and processed on the subscription
        /// </summary>
        private readonly ConcurrentDictionary<Guid, TaskCompletionSource<object>> _pendingLocalCommands;

        private readonly AutoResetEvent _pendingCommandsChanged = new AutoResetEvent(false);

        private readonly IEngineMetrics _metrics;


        /// <summary>
        /// Last record number applied to the model, -1 if initial model
        /// Numbering starts from 0!
        /// </summary>
        private long _lastRecordNumber = -1;

        /// <summary>
        /// Unique identifier of this running engine
        /// </summary>
        public readonly Guid EngineId = Guid.NewGuid();

        private readonly IJournalReader _journalReader;
        private CancellationTokenSource _readerCancellationTokenSource;
        private TaskCompletionSource<object> _readyTask;
        public event CommandExecuted CommandExecuted = delegate { };

        public event StateTransitioned StateChanged = delegate { };

        public Engine(
            TModel model,
            EngineSettings settings,
            IStorageProvider storageProvider)
        {
            _logger = LogProvider.GetCurrentClassLogger();
            _kernel = new Kernel(settings, model);
            _settings = settings;
            _journalWriter = storageProvider.CreateJournalWriter();
            _journalReader = storageProvider.CreateJournalReader();
            _pendingLocalCommands = new ConcurrentDictionary<Guid, TaskCompletionSource<object>>();
            _metrics = Metrics.Provider.GetEngineMetrics();
        }

        public Task Start(bool waitUntilReady = false)
        {
            if (State == EngineState.Running || State == EngineState.Loading) 
                throw new InvalidOperationException("Already started");

            _readyTask = new TaskCompletionSource<object>();
            _readerCancellationTokenSource = new CancellationTokenSource();
            var token = _readerCancellationTokenSource.Token;
            _readerTask = _journalReader.Subscribe(_lastRecordNumber, OnRecordReceived, token);
            return waitUntilReady ? _readyTask.Task : _readerTask;
        }

        public long LastRecordNumber => Interlocked.Read(ref _lastRecordNumber);

        public async Task<TResult> Execute<TResult>(Command<TModel, TResult> command)
        {
            return (TResult) await ExecuteUntyped(command);
        }

        public Task Execute(Command<TModel> command)
        {
            return ExecuteUntyped(command);
        }

        public Task<TResult> Execute<TResult>(Query<TModel, TResult> query)
        {
            var result = (TResult)ExecuteUntyped(query);
            return Task.FromResult(result);
        }

        public async Task DisposeAsync()
        {
            if (State == EngineState.Disposed || State == EngineState.Disposing) return;
            
            _logger.Debug("Disposing...");

            _logger.Info("Stopping JournalWriter");
            await _journalWriter.DisposeAsync().NotOnCapturedContext();
            
            _logger.Info("Waiting for pending commands");
            while (!_pendingLocalCommands.IsEmpty) _pendingCommandsChanged.WaitOne();

            _logger.Info("Stopping JournalReader");
            _readerCancellationTokenSource.Cancel();
            await _readerTask;

            _logger.Debug("Dispose completed");
        }

        /// <summary>
        /// Wait until a specific record has beed executed
        /// </summary>
        /// <param name="recordNumber">The version </param>
        /// <returns></returns>
        public async Task EnsureVersion(long recordNumber)
        {
            EnsureState("EnsureVersion()", EngineState.Loading, EngineState.Running);

            while (LastRecordNumber < recordNumber)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(10)).NotOnCapturedContext();
            }
        }

        internal object ExecuteUntyped(Query query)
        {
            EnsureState("Execute(Query)", EngineState.Running, EngineState.Stopped);

            using (_metrics.MeasureQueryExecution())
            {
                try
                {
                    var result = _kernel.Execute(query);
                    _metrics.QueryExecuted();
                    return result;
                }
                catch (Exception)
                {
                    _metrics.QueryFailed();
                    throw;
                }
            }
        }

        internal void SetStateAndNotify(EngineState newState)
        {
            var oldState = State;
            State = newState;
            StateChanged(oldState, newState);
        }

        /// <summary>
        /// Execute non-generically typed command
        /// </summary>
        internal async Task<object> ExecuteUntyped(Command command)
        {
            EnsureState("Execute(Command)", EngineState.Loading, EngineState.Running);

            var completionSource = new TaskCompletionSource<object>();
            _pendingLocalCommands[command.CommandId] = completionSource;

            _metrics.PendingLocalCommands(_pendingLocalCommands.Count);

            using (_metrics.MeasureCommandExecution())
            {
                await _journalWriter.Write(command);
                return completionSource.Task;
            }
        }

        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        private void EnsureState(string operation, params EngineState[] states)
        {
            // ReSharper disable once SimplifyLinqExpression
            if (!states.Contains(State)) 
                throw new InvalidOperationException($"{operation} invalid in the {State} state");
        }

        /// <summary>
        /// Handler for records obtained from the reader
        /// </summary>
        private void OnRecordReceived(JournalRecord record)
        {

            if (State == EngineState.Faulted) return;
            TaskCompletionSource<object> completion = null;

            try
            {
                var command = record.Command;
                var isLocalCommand = _pendingLocalCommands.TryRemove(command.CommandId, out completion);

                if (isLocalCommand)
                {
                    _metrics.PendingLocalCommands(_pendingLocalCommands.Count);
                    _pendingCommandsChanged.Set();
                }

                _logger.Debug("OnRecordReceived: {0}/{1}, isLocal: {2}", record.RecordNumber, command.GetType().Name, isLocalCommand);

                VerifyRecordSequence(record.RecordNumber);

                var ctx = ExecutionContext.Current;
                ctx.Reset(record.RecordNumber);

                object result = null;
                if (record.Command is ControlCommand<TModel> cc) cc.Execute(this);
                else result = _kernel.Execute(record.Command);

                Interlocked.Exchange(ref _lastRecordNumber, record.RecordNumber);
                NotifyCommandExecuted(record, isLocalCommand, ctx.Events);

                completion?.SetResult(result);
                _metrics.CommandExecuted();
            }
            catch (Exception ex)
            {
                _metrics.CommandFailed();
                _logger.Error(ex, "OnRecordReceived failed: {0}/{1}", record.RecordNumber, record.Command.GetType().Name);
                completion?.SetException(ex);
            }
        }

        private void VerifyRecordSequence(long actualRecordNumber)
        {
            var expected = Interlocked.Read(ref _lastRecordNumber) + 1;
            if (expected != actualRecordNumber)
            {
                if (!_settings.AllowBrokenSequence)
                {
                    State = EngineState.Faulted;
                    throw new Exception($"Broken sequence, expected {expected}, got {actualRecordNumber}");
                }

                _logger.Warn(
                    "OnRecordReceived: RecordNumber out of order. Expected {0}, got {1}",
                    expected,
                    actualRecordNumber);
            }
        }
        
        private void NotifyCommandExecuted(JournalRecord journalRecord, bool isLocal, IEnumerable<Event> events)
        {
            try
            {
                CommandExecuted.Invoke(journalRecord, isLocal, events);
            }
            catch (Exception exception)
            {
                _logger.Error(exception, "Exception thrown in CommandExecuted handler.");
            }
        }
    }
}