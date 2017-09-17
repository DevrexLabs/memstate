using System;

namespace Memstate.Tcp
{
    internal class Response : Message
    {
        public Response(Guid responseTo)
        {
            ResponseTo = responseTo;
        }

        public Guid ResponseTo { get; }

    }

    internal class QueryResponse : Response
    {
        public QueryResponse(object result, Guid responseTo)
            : base(responseTo)
        {
            Result = result;
        }

        public object Result { get; }
    }

    internal class CommandResponse : Response
    {
        public CommandResponse(object result, Guid responseTo)
            : base(responseTo)
        {
            Result = result;
        }

        public object Result { get; }
    }

    internal class CommandRequest : Request
    {
        public CommandRequest(Command command)
        {
            Command = command;
        }

        public Command Command { get; set; }

    }

    internal abstract class Request : Message
    {
    }

    internal class QueryRequest : Request
    {
        public QueryRequest(Query query)
        {
            Query = query;
        }

        public Query Query { get; }

    }

    internal class Ping : Request
    {
        public Ping(Guid id)
        {
            Id = id;
        }

        public Ping() : this(Guid.NewGuid())
        {
            
        }

    }

    internal class Pong : Response
    {
        public Pong(Ping ping) : base(ping.Id)
        {
        }
    }

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
