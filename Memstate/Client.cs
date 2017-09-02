using System.Threading.Tasks;

namespace Memstate
{
    public abstract class Client<TModel>
    {
        internal abstract object Execute(Query query);

        public abstract void Execute(Command<TModel> command);

        public abstract TResult Execute<TResult>(Command<TModel, TResult> command);

        public abstract TResult Execute<TResult>(Query<TModel, TResult> query);
        
        public abstract Task ExecuteAsync(Command<TModel> command);

        public abstract Task<TResult> ExecuteAsync<TResult>(Command<TModel, TResult> command);

        public abstract Task<TResult> ExecuteAsync<TResult>(Query<TModel, TResult> query);
    }

    public class MemstateTcpClient<TModel> : Client<TModel> where TModel : class
    {

        public MemstateTcpClient(string endpoint)
        {
        }

        internal override object Execute(Query query)
        {
            throw new System.NotImplementedException();
        }

        public override void Execute(Command<TModel> command)
        {
            throw new System.NotImplementedException();
        }

        public override TResult Execute<TResult>(Command<TModel, TResult> command)
        {
            throw new System.NotImplementedException();
        }

        public override TResult Execute<TResult>(Query<TModel, TResult> query)
        {
            throw new System.NotImplementedException();
        }

        public override Task ExecuteAsync(Command<TModel> command)
        {
            throw new System.NotImplementedException();
        }

        public override Task<TResult> ExecuteAsync<TResult>(Command<TModel, TResult> command)
        {
            throw new System.NotImplementedException();
        }

        public override Task<TResult> ExecuteAsync<TResult>(Query<TModel, TResult> query)
        {
            throw new System.NotImplementedException();
        }
    }

    public class LocalClient<TModel> : Client<TModel> where TModel : class
    {
        private readonly Engine<TModel> _engine;

        public LocalClient(Engine<TModel> engine)
        {
            _engine = engine;
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
    }
}