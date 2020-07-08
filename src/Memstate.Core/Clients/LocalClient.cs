using System;
using System.Threading.Tasks;

namespace Memstate
{

    /// <summary>
    /// LocalClient has a direct reference to an Engine running in
    /// the current process.
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    public class LocalClient<TModel> : Client<TModel> where TModel : class
    {
        private readonly Engine<TModel> _engine;

        private readonly EventHandlers<TModel> _eventHandlers;

        public LocalClient(Engine<TModel> engine)
        {
            _engine = engine;
            _eventHandlers = new EventHandlers<TModel>(_engine);
        }

        internal override Task<object> ExecuteUntyped(Query query) 
            => Task.FromResult(_engine.ExecuteUntyped(query));

        public override Task Execute(Command<TModel> command)
        {
            return _engine.Execute(command);
        }

        public override Task<TResult> Execute<TResult>(Command<TModel, TResult> command)
        {
            return _engine.Execute(command);
        }

        public override Task<TResult> Execute<TResult>(Query<TModel, TResult> query)
        {
            return _engine.Execute(query);
        }

        public override Task Unsubscribe<T>()
        {
            _eventHandlers.ClearHandler<T>();
            return Task.CompletedTask;
        }

        public override Task DisposeAsync() => _engine.DisposeAsync();

        public override Task Subscribe<T>(Action<T> handler, IEventFilter filter = null)
        {
            _eventHandlers.SetHandler(handler, filter);
            return Task.CompletedTask;
        }
    }
}