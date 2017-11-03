using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Memstate
{
    public class Engine<TModel> : IDisposable where TModel : class
    {
        private readonly ILogger _logger;
        private readonly Kernel _kernel;
        private readonly IJournalWriter _journalWriter;
        private readonly ConcurrentDictionary<Guid, TaskCompletionSource<object>> _pendingLocalCommands;
        private readonly IDisposable _commandSubscription;
        private readonly AutoResetEvent _pendingCommandsChanged = new AutoResetEvent(false);
        private long _lastRecordNumber;

        public Engine(
            MemstateSettings config,
            TModel model,
            IJournalSubscriptionSource subscriptionSource,
            IJournalWriter journalWriter,
            long nextRecord)
        {
            _lastRecordNumber = nextRecord - 1;
            _logger = config.CreateLogger<Engine<TModel>>();
            _kernel = new Kernel(config, model);
            _journalWriter = journalWriter;
            _pendingLocalCommands = new ConcurrentDictionary<Guid, TaskCompletionSource<object>>();
            _commandSubscription = subscriptionSource.Subscribe(nextRecord, ApplyRecord);
        }

        public async Task<TResult> ExecuteAsync<TResult>(Command<TModel, TResult> command)
        {
            var completionSource = new TaskCompletionSource<object>();
            _pendingLocalCommands[command.Id] = completionSource;
            _journalWriter.Send(command);
            return (TResult)await completionSource.Task.ConfigureAwait(false);
        }

        public Task ExecuteAsync(Command<TModel> command)
        {
            var completionSource = new TaskCompletionSource<object>();

            _pendingLocalCommands[command.Id] = completionSource;
            _journalWriter.Send(command);

            return completionSource.Task;
        }

        public async Task<TResult> ExecuteAsync<TResult>(Query<TModel, TResult> query)
        {
            return await Task.Run(() => Execute(query)).ConfigureAwait(false);
        }

        public TResult Execute<TResult>(Command<TModel, TResult> command)
        {
            return ExecuteAsync(command).Result;
        }

        public void Execute(Command<TModel> command)
        {
            ExecuteAsync(command).Wait();
        }

        public TResult Execute<TResult>(Query<TModel, TResult> query)
        {
            return (TResult)_kernel.Execute(query);
        }

        public void Dispose()
        {
            _logger.LogDebug("Begin Dispose");
            _journalWriter.Dispose();
            while (!_pendingLocalCommands.IsEmpty)
            {
                _pendingCommandsChanged.WaitOne();
            }

            _commandSubscription.Dispose();
            _logger.LogDebug("End Dispose");
        }

        internal object Execute(Query query)
        {
            return _kernel.Execute(query);
        }

        internal object Execute(Command command)
        {
            return _kernel.Execute(command);
        }

        /// <summary>
        /// Handler for records obtained through the subscription
        /// </summary>
        private void ApplyRecord(JournalRecord record)
        {
            TaskCompletionSource<object> completion = null;
            try
            {
                var command = record.Command;
                var isLocalCommand = _pendingLocalCommands.TryRemove(command.Id, out completion);
                if (isLocalCommand)
                {
                    _pendingCommandsChanged.Set();
                }

                _logger.LogDebug("ApplyRecord: {0}/{1}, isLocal: {2}", record.RecordNumber, command.GetType().Name, isLocalCommand);
                long expected = Interlocked.Increment(ref _lastRecordNumber);
                if (expected != record.RecordNumber)
                {
                    _logger.LogError("ApplyRecord: RecordNumber out of order. Expected {0}, got {1}", expected, record.RecordNumber);
                }

                object result = _kernel.Execute(record.Command);
                completion?.SetResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(default(EventId), ex, "ApplyRecord failed: {0}/{1}", record.RecordNumber, record.Command.GetType().Name);
                completion?.SetException(ex);
            }
        }
    }
}