using System;
using Newtonsoft.Json;

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

    internal class EventsResponse : Message
    {
        public EventsResponse(Event[] events)
        {
            Events = events;
        }

        [JsonProperty]
        public Event[] Events { get; private set; }
    }

    internal class SubscribeRequest : Request
    {
        public SubscribeRequest(Type type, IEventFilter[] filters)
        {
            Type = type;
            Filters = filters;
        }

        [JsonProperty]
        public Type Type { get; private set; }

        [JsonProperty]
        public IEventFilter[] Filters { get; private set; }
    }

    internal class SubscribeResponse : Response
    {
        public SubscribeResponse(Guid responseTo)
            : base(responseTo)
        {
        }
    }

    internal class UnsubscribeRequest : Request
    {
        public UnsubscribeRequest(Type type)
        {
            Type = type;
        }

        [JsonProperty]
        public Type Type { get; private set; }
    }

    internal class UnsubscribeResponse : Response
    {
        public UnsubscribeResponse(Guid responseTo)
            : base(responseTo)
        {
        }
    }

    internal class FilterRequest : Request
    {
        public FilterRequest(IEventFilter filter)
        {
            Filter = filter;
        }

        [JsonProperty]
        public IEventFilter Filter { get; private set; }
    }

    internal class FilterResponse : Response
    {
        public FilterResponse(Guid responseTo)
            : base(responseTo)
        {
        }
    }
}