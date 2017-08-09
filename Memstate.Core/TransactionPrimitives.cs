using System;

namespace Memstate.Core
{
    public abstract class Command<TModel, TResult> : Command
    {
        public abstract TResult Execute(TModel model);

        public override object ExecuteImpl(object model)
        {
            return Execute((TModel) model);
        }
    }

    public abstract class Command<TModel> : Command
    {
        public abstract void Execute(TModel model);

        public override object ExecuteImpl(object model)
        {
            Execute((TModel) model);
            
            return null;
        }
    }

    public abstract class Query<TModel, TResult> : Query
    {
        public abstract TResult Execute(TModel model);

        public override object ExecuteImpl(object model)
        {
            return Execute((TModel) model);
        }
    }

    public abstract class Query
    {
        public abstract object ExecuteImpl(object model);
    }

    public abstract class Command
    {
        protected Command()
        {
            Id = Guid.NewGuid();
        }
        public Guid Id { get; set; } 
        public abstract object ExecuteImpl(object model);
    }
}