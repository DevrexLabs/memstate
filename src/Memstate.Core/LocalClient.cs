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
            : this(new EngineBuilder(settings).Build(creator()))
        {
        }

        internal override object Execute(Query query)
        {
            return _engine.Execute(query);
        }

        public override void Execute(Command<TModel> command)
        {
            _engine.Execute(command);
        }

        public override TResult Execute<TResult>(Command<TModel, TResult> command)
        {
            return _engine.Execute(command);
        }

        public override TResult Execute<TResult>(Query<TModel, TResult> query)
        {
            return _engine.Execute(query);
        }

        public override Task ExecuteAsync(Command<TModel> command)
        {
            return _engine.ExecuteAsync(command);
        }

        public override Task<TResult> ExecuteAsync<TResult>(Command<TModel, TResult> command)
        {
            return _engine.ExecuteAsync(command);
        }

        public override Task<TResult> ExecuteAsync<TResult>(Query<TModel, TResult> query)
        {
            return _engine.ExecuteAsync(query);
        }


        public override Task UnsubscribeAsync<T>()
        {
            _eventHandlers.ClearHandler<T>();
            return Task.CompletedTask;
        }

        public override Task SubscribeAsync<T>(Action<T> handler, IEventFilter filter = null)
        {
            _eventHandlers.SetHandler<T>(handler, filter);
            return Task.CompletedTask;
        }
    }
}