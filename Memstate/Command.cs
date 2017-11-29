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
        
        public abstract object ExecuteImpl(object model, Action<Event> raise);
    }
    
    public abstract class Command<TModel> : Command
    {
        public virtual void Execute(TModel model, Action<Event> raise)
        {
            if (raise == null)
            {
                throw new NotSupportedException("One of Execute(TModel, Action<Event>) or Execute(TModel) must be overridden.");
            }
            
            Execute(model);
        }

        public virtual void Execute(TModel model)
        {
            Execute(model, null);
        }

        public override object ExecuteImpl(object model, Action<Event> raise)
        {
            Execute((TModel) model, raise);

            return null;
        }
    }
    
    public abstract class Command<TModel, TResult> : Command
    {
        public virtual TResult Execute(TModel model, Action<Event> raise)
        {
            if (raise == null)
            {
                throw new NotSupportedException("One of Execute(TModel, Action<Event>) or Execute(TModel) must be overridden.");
            }
            
            return Execute(model);
        }

        public virtual TResult Execute(TModel model)
        {
            return Execute(model, null);
        }

        public override object ExecuteImpl(object model, Action<Event> raise)
        {
            return Execute((TModel) model, raise);
        }
    }
}