namespace Memstate
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;

    public static class ExtensionMethods
    {
        public static T TakeOrDefault<T>(this BlockingCollection<T> collection, CancellationToken cancellationToken)
        {
            try
            {
                return collection.Take(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                return default(T);
            }
        }

        public static MemstateSettings WithInmemoryStorage(this MemstateSettings settings)
        {
            settings.StorageProvider = typeof(InMemoryStorageProvider).FullName;
            return settings;
        }

        public static MemstateSettings WithRandomStreamName(this MemstateSettings settings)
        {
            settings.StreamName = "stream-" + Guid.NewGuid();
            return settings;
        }
    }
}
