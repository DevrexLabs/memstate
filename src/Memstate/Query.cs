namespace Memstate
{
    public abstract class Query
    {
        public abstract object ExecuteImpl(object model);
    }

    public abstract class Query<TModel, TResult> : Query
    {
        public abstract TResult Execute(TModel db);

        public bool? ResultIsIsolated { get; set; }

        public override object ExecuteImpl(object model)
        {
            return Execute((TModel) model);
        }
    }
}