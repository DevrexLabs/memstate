using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Memstate.Core
{
    public class Engine<TModel> : IDisposable where TModel : class
    {
        private readonly ILogger _logger;
        private readonly Kernel _kernel;
        private readonly IJournalWriter _journalWriter;
        private readonly ConcurrentDictionary<Guid, TaskCompletionSource<object>> _pendingLocalCommands;
        private readonly IDisposable _commandSubscription;

        public Engine(
            Config config,
            TModel model,
            IJournalSubscriptionSource subscriptionSource,
            IJournalWriter journalWriter,
            long nextRecord)
        {
            _logger = config.CreateLogger<Engine<TModel>>();
            _kernel = new Kernel(config, model);
            _journalWriter = journalWriter;
            _pendingLocalCommands = new ConcurrentDictionary<Guid, TaskCompletionSource<object>>();
            _commandSubscription = subscriptionSource.Subscribe(nextRecord, ApplyRecord);
        }

        private void ApplyRecord(JournalRecord record)
        {
                TaskCompletionSource<object> completion = null;
                try
                {
                    var command = record.Command;
                    var isLocalCommand = _pendingLocalCommands.TryRemove(command.Id, out completion);
                    _logger.LogDebug("ApplyRecord: {0}/{1}, isLocal: {2}", record.RecordNumber, command.GetType().Name, isLocalCommand);
                    object result = _kernel.Execute(record.Command);
                    completion?.SetResult(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(default(EventId), ex, "ApplyRecord failed: {0}/{1}", record.RecordNumber, record.Command.GetType().Name);
                    completion?.SetException(ex);   
                }
        }

        public async Task<TResult> ExecuteAsync<TResult>(Command<TModel, TResult> command)
        {
            var completionSource = new TaskCompletionSource<object>();

            _pendingLocalCommands[command.Id] = completionSource;
            _journalWriter.Send(command);
            //return await Task.FromResult(default(TResult));
            return (TResult) await completionSource.Task;
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
            return await Task.Run(() => Execute(query));
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
            return (TResult) _kernel.Execute(query);
        }

        public void Dispose()
        {
            _logger.LogDebug("Begin Dispose");
            _journalWriter.Dispose();
            _commandSubscription.Dispose();
            _logger.LogDebug("End Dispose");
        }
    }
}