namespace Memstate
{
    public interface IEventFilter
    {
        bool Accept(Event item);
    }
}