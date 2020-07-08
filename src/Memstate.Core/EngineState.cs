namespace Memstate
{
    public enum EngineState
    {
        /// <summary>
        /// Has not yet been started, call Start()
        /// </summary>
        NotStarted,

        /// <summary>
        /// Journal is being replayed to bring the model up to date
        /// </summary>
        Loading,

        /// <summary>
        /// Loading 
        /// </summary>
        Running,
        
        /// <summary>
        /// Stop() has been called but not yet reached the Stopped state
        /// </summary>
        Stopping,

        /// <summary>
        /// Stop() was called so subscription is paused
        /// </summary>
        Stopped,

        /// <summary>
        /// Stopped due to an error condition
        /// </summary>
        Faulted,

        /// <summary>
        /// Dispose has been called but is not yet completed
        /// </summary>
        Disposing,

        /// <summary>
        /// Dispose has completed
        /// </summary>
        Disposed
    }
}