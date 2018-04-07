using System;

namespace Memstate
{
    public abstract class Command
    {
        protected Command()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }

        public abstract object ExecuteImpl(object model);

        protected void RaiseEvent(Event @event) {
            EventRaised.Invoke(@event);
        }

        public event Action<Event> EventRaised = _ => { };
    }

    public abstract class Command<TModel> : Command
    {

        public virtual void Execute(TModel model)
        {
            Execute(model);
        }

        public override object ExecuteImpl(object model)
        {
            Execute((TModel) model);

            return null;
        }
    }

    public abstract class Command<TModel, TResult> : Command
    {
        public virtual TResult Execute(TModel model)
        {
            return Execute(model);
        }

        public override object ExecuteImpl(object model)
        {
            return Execute((TModel) model);
        }
    }
}