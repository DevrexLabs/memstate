using System.Threading.Tasks;

namespace Memstate
{
    public abstract class Client<TModel>
    {
        public abstract void Execute(Command<TModel> command);

        public abstract TResult Execute<TResult>(Command<TModel, TResult> command);

        public abstract TResult Execute<TResult>(Query<TModel, TResult> query);

        public abstract Task ExecuteAsync(Command<TModel> command);

        public abstract Task<TResult> ExecuteAsync<TResult>(Command<TModel, TResult> command);

        public abstract Task<TResult> ExecuteAsync<TResult>(Query<TModel, TResult> query);

        public abstract IClientEvents Events { get; }

        internal abstract object Execute(Query query);
    }
}