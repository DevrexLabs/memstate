using System.Threading;
using Memstate;

namespace Memstate3
{
    abstract class Input<TState, TResult>
    {
        public abstract (TState, TResult, Event[]) Apply(TState state);
    }
    
    public sealed class None
    {
        public static None Instance = new None();

        private None()
        {
            
        }
    } 
    
    abstract class Input<TState> : Input<TState, None>
    {
        public override (TState, None, Event[]) Apply(TState state)
        {
            var (newState, events) = ApplyImpl(state);
            return (newState, None.Instance, events);
        }

        protected abstract (TState, Event[]) ApplyImpl(TState state);
    }

    internal class ReduxKernel<TState> where TState: class
    {
        private TState _state;

        
        public (TResult, Event[]) Apply<TResult>(Input<TState, TResult> input)
        {
            var (newState, result, outputs) = input.Apply(_state);
            Interlocked.Exchange(ref _state, newState);
            _state = newState;
            return (result, outputs);
        }

        public TResult Apply<TResult>(Query<TState, TResult> query)
        {
            var state = Interlocked.Exchange(ref _state, _state);
            var result = query.Execute(state);
            return result;
        }

    }
}