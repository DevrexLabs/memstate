using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Memstate.Configuration;
using Memstate.Logging;

namespace Memstate
{
    public class Engine<TModel> where TModel : class
    {
        public EngineState State { get; private set; } 
            = EngineState.NotStarted;

        // ReSharper disable once StaticMemberInGenericType
        private readonly ILog _log = LogProvider.GetLogger(nameof(Engine));

        private readonly Kernel _kernel;

        private readonly EngineSettings _settings;

        private readonly IJournalWriter _journalWriter;
        
        private Task _subscription;
        private CancellationTokenSource _subscriptionCancellation;
        private TaskCompletionSource<object> _subscriptionCaughtUp;
        
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
        
        public event CommandExecuted CommandExecuted = delegate { };
        public event StateTransitioned StateChanged = delegate { };

        public Engine(TModel model, Config config)
        {
            _settings = config.GetSettings<EngineSettings>();
            _kernel = new Kernel(_settings, model);
            
            var storageProvider = config.GetStorageProvider();
            _journalWriter = storageProvider.CreateJournalWriter();
            _journalReader = storageProvider.CreateJournalReader();
            
            _pendingLocalCommands = new ConcurrentDictionary<Guid, TaskCompletionSource<object>>();
            _metrics = Metrics.Provider.GetEngineMetrics();
        }


        public async Task Start(bool waitUntilReady = true)
        {
            EnsureState("Start()", EngineState.Stopped, EngineState.NotStarted);
            SetStateAndNotify(EngineState.Loading);

            //Start the journal subscription task
            _subscriptionCancellation = new CancellationTokenSource();
            var token = _subscriptionCancellation.Token;
            _subscription = _journalReader.Subscribe(_lastRecordNumber, OnRecordReceived, token);

            // Issue a control command which when it arrives on the
            // subscription we know we are caught up. This will cause
            // a state transition from Loading to Running
            _subscriptionCaughtUp = new TaskCompletionSource<object>();
            var command = new SetStateToRunning<TModel>(EngineId);
            await _journalWriter.Write(command).NotOnCapturedContext();

            if (waitUntilReady) await _subscriptionCaughtUp.Task.NotOnCapturedContext();
        }

        public async Task Stop()
        {
            EnsureState("Stop()", EngineState.Running, EngineState.Loading);
            SetStateAndNotify(EngineState.Stopping);
            _subscriptionCancellation.Cancel();
            await _subscription.NotOnCapturedContext();
            SetStateAndNotify(EngineState.Stopped);
        }

        internal void OnSubscriptionCaughtUp()
        {
            SetStateAndNotify(EngineState.Running);
            _subscriptionCaughtUp.SetResult(0);
        }

        public long LastRecordNumber => Interlocked.Read(ref _lastRecordNumber);

        public async Task<TResult> Execute<TResult>(Command<TModel, TResult> command)
        {
            return (TResult) await ExecuteUntyped(command).NotOnCapturedContext();
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
            if (State == EngineState.NotStarted)
            {
                SetStateAndNotify(EngineState.Disposed);
                return;
            }

            SetStateAndNotify(EngineState.Disposing);
            _log.Info("Stopping JournalWriter");
            await _journalWriter.DisposeAsync().NotOnCapturedContext();
            
            _log.Info("Waiting for pending commands");
            while (!_pendingLocalCommands.IsEmpty) _pendingCommandsChanged.WaitOne();

            _log.Info("Stopping JournalReader");
            _subscriptionCancellation.Cancel();
            await _subscription.NotOnCapturedContext();
            SetStateAndNotify(EngineState.Disposed);
        }

        /// <summary>
        /// Wait until a specific record has been executed
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
            _log.Info($"State changed from {oldState} to {newState}");
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
                await _journalWriter.Write(command).NotOnCapturedContext();
                return await completionSource.Task.NotOnCapturedContext();
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

                _log.Debug("OnRecordReceived: {0}/{1}, isLocal: {2}", record.RecordNumber, command.GetType().Name, isLocalCommand);

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
                _log.Error(ex, "OnRecordReceived failed: {0}/{1}", record.RecordNumber, record.Command.GetType().Name);
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

                _log.Warn(
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
                _log.Error(exception, "Exception thrown in CommandExecuted handler.");
            }
        }
    }
}