namespace Memstate
{
    public interface IEventFilter
    {
        /// <summary>
        /// Accept or decline the event for further processing.
        /// </summary>
        /// <param name="item">
        /// The event to check.
        /// </param>
        /// <returns>
        /// <c>true</c> if the event is accepted; otherwise <c>false</c>.
        /// </returns>
        bool Accept(Event item);
    }
}