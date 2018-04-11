namespace Memstate.Tcp
{
    internal class QueryRequest : Request
    {
        public QueryRequest(Query query)
        {
            Query = query;
        }

        public Query Query { get; }
    }
}