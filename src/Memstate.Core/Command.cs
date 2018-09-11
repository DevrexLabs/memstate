using System;

namespace Memstate
{
    public abstract class Command
    {
        protected Command()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; internal set; }

        internal abstract object ExecuteImpl(object model);

        protected void RaiseEvent(Event @event) {
            ExecutionContext.Current.AddEvent(@event);
        }
    }

    public abstract class Command<TModel> : Command
    {
        public abstract void Execute(TModel model);

        internal override object ExecuteImpl(object model)
        {
            Execute((TModel) model);
            return null;
        }
    }

    public abstract class Command<TModel, TResult> : Command
    {
        public abstract TResult Execute(TModel model);

        internal override object ExecuteImpl(object model)
        {
            return Execute((TModel) model);
        }
    }
}