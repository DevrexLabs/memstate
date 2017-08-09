using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Memstate.Core
{
    public class Engine<TModel> : IDisposable where TModel : class
    {
        private readonly Kernel _kernel;
        private readonly IJournalWriter _journalWriter;
        private readonly ConcurrentDictionary<Guid, TaskCompletionSource<object>> _pendingLocalCommands;
        private readonly IDisposable _commandSubscription;

        public Engine(
            TModel model,
            IJournalSubscriptionSource subscriptionSource,
            IJournalWriter journalWriter,
            long nextRecord)
        {
            _kernel = new Kernel(model);
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
                    _pendingLocalCommands.TryRemove(command.Id, out completion);
                    object result = _kernel.Execute(record.Command);
                    completion?.SetResult(result);
                }
                catch (Exception ex)
                {
                    //todo: log
                    completion?.SetException(ex);   
                }
        }

        public async Task<TResult> ExecuteAsync<TResult>(Command<TModel, TResult> command)
        {
            var completionSource = new TaskCompletionSource<object>();

            _pendingLocalCommands[command.Id] = completionSource;
            _journalWriter.AppendAsync(command);

            return (TResult) await completionSource.Task;
        }

        public Task ExecuteAsync(Command<TModel> command)
        {
            var completionSource = new TaskCompletionSource<object>();

            _pendingLocalCommands[command.Id] = completionSource;
            _journalWriter.AppendAsync(command);

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
            _journalWriter.Dispose();
            _commandSubscription.Dispose();
        }
    }
}