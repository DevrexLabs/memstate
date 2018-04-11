using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Memstate
{
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
            settings.FileSystem = new InMemoryFileSystem();

            return settings;
        }

        public static MemstateSettings WithRandomSuffixAppendedToStreamName(this MemstateSettings settings)
        {
            var randomPart = Guid.NewGuid().ToString("N").Substring(0, 10);

            settings.StreamName += randomPart;

            return settings;
        }

        public static async Task<string> ReadToEnd(this Stream stream)
        {
            using (stream)
            using (var reader = new StreamReader(stream))
            {
                return await reader.ReadToEndAsync().ConfigureAwait(false);
            }
        }
    }
}