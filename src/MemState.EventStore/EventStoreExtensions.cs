using EventStore.ClientAPI;

namespace Memstate.EventStore
{
    public static class EventStoreExtensions
    {
        public static JournalRecord ToJournalRecord(this RecordedEvent @event, ISerializer serializer)
        {
            var command = (Command) serializer.Deserialize(@event.Data);
            return new JournalRecord(@event.EventNumber, @event.Created, command);
        }
    }
}