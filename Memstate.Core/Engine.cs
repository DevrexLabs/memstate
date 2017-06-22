using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Memstate.Core
{
    public class Engine<TModel> : IDisposable where TModel : class
    {
        private readonly Kernel _kernel;
        private readonly ICommandLogger _commandLogger;
        private readonly ConcurrentDictionary<Guid, object> _pendingLocalCommands;
        private readonly IDisposable _commandLoggedSubscription;

        public Engine(
            TModel model,
            ICommandSubscriptionSource subscriptionSource,
            ICommandLogger commandLogger,
            ulong version)
        {
            _kernel = new Kernel(model, version);
            _commandLogger = commandLogger;
            _commandLoggedSubscription = subscriptionSource.Subscribe(version + 1, ApplyCommand);
            _pendingLocalCommands = new ConcurrentDictionary<Guid, object>();
        }

        private void ApplyCommand(Command command)
        {
            object result = _kernel.Execute(command);

            if (_pendingLocalCommands.TryRemove(command.Id, out var completion))
            {
                ((TaskCompletionSource<object>) completion).SetResult(result);
            }
        }

        public async Task<TResult> ExecuteAsync<TResult>(Command<TModel, TResult> command)
        {
            var tcs = new TaskCompletionSource<object>();

            _pendingLocalCommands[command.Id] = tcs;
            _commandLogger.Append(command);

            return (TResult) await tcs.Task;
        }

        public Task ExecuteAsync(Command<TModel> command)
        {
            var tcs = new TaskCompletionSource<object>();

            _pendingLocalCommands[command.Id] = tcs;
            _commandLogger.Append(command);

            return tcs.Task;
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
            _commandLogger.Dispose();
            _commandLoggedSubscription.Dispose();
        }
    }
}