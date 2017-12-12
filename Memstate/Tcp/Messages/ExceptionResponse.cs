using System;

namespace Memstate.Tcp
{
    internal class ExceptionResponse : Response
    {
        public Exception Exception { get; }

        public ExceptionResponse(Message cause, Exception exception)
            : base(cause.Id)
        {
            Exception = exception;
        }
    }
}