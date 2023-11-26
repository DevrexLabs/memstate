using System;

namespace Memstate
{
    [Serializable]
    public abstract class Command
    {
        [NonSerialized]
        internal Action<Event> EventRaised;
        
//        internal ExecutionContext Context;
        internal abstract object ExecuteImpl(object model);

        protected void RaiseEvent(Event @event)
        {
            if (EventRaised != null) EventRaised.Invoke(@event);
        }
    }

    [Serializable]
    public abstract class Command<TModel> : Command
    {
        public abstract void Execute(TModel model);

        internal override object ExecuteImpl(object model)
        {
            Execute((TModel) model);
            return null;
        }
    }

    [Serializable]
    public abstract class Command<TModel, TResult> : Command
    {
        public abstract TResult Execute(TModel model);

        internal override object ExecuteImpl(object model)
        {
            return Execute((TModel) model);
        }
    }
}