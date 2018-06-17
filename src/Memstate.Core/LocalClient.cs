using System;
using System.Threading.Tasks;

namespace Memstate
{

    public class LocalClient<TModel> : Client<TModel> where TModel : class
    {
        private readonly Engine<TModel> _engine;

        private readonly EventHandlers<TModel> _eventHandlers;

        public LocalClient(Engine<TModel> engine)
        {
            _engine = engine;
            _eventHandlers = new EventHandlers<TModel>(_engine);
        }

        public LocalClient(Func<TModel> creator, MemstateSettings settings)
            : this(new EngineBuilder(settings).Build(creator()).Result)
        {
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

        public override Task Subscribe<T>(Action<T> handler, IEventFilter filter = null)
        {
            _eventHandlers.SetHandler<T>(handler, filter);
            return Task.CompletedTask;
        }
    }
}