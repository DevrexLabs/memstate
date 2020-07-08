using System;

namespace Memstate
{

    /// <summary>
    /// Control commands are used by the engine for 
    /// </summary>
    [Serializable]
    internal abstract class ControlCommand<T> : Command<Engine<T>> where T : class
    {
        /// <summary>
        /// The engine that issued this command
        /// </summary>
        public Guid EngineId { get; set; }    
    }

    internal class SetStateToRunning<T> : ControlCommand<T> where T: class
    {
        public SetStateToRunning(Guid engineId)
        {
            EngineId = engineId;
        }

        public override void Execute(Engine<T> engine)
        {
            if (engine.EngineId == EngineId)
            {
                engine.OnSubscriptionCaughtUp();
            }
        }
    }
    
    

    [Serializable]
    public abstract class Command
    {
        protected Command()
        {
            CommandId = Guid.NewGuid();
        }

        public Guid CommandId { get; internal set; }

        internal abstract object ExecuteImpl(object model);

        protected void RaiseEvent(Event @event) {
            ExecutionContext.Current.AddEvent(@event);
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