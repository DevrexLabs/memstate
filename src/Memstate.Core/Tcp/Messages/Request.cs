namespace Memstate.Tcp
{
    /// <summary>
    /// A request is a <c>Message</c> that, when handled, should return a response <c>Response</c>
    /// </summary>
    internal abstract class Request : Message
    {
    }
}