using System;

namespace Memstate.Tcp
{
    /// <summary>
    /// A response is an acknowledgement that a <c>Request</c> has been handled.
    /// If a <c>Request</c> returns data created a type derived from <c>Response</c>
    /// </summary>
    internal class Response : Message
    {
        public Response(Guid responseTo)
        {
            ResponseTo = responseTo;
        }

        /// <summary>
        /// Id of the <c>Request</c> corresponding to this <c>Response</c>
        /// </summary>
        public Guid ResponseTo { get; }
    }
}