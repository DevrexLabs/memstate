using NUnit.Framework;

namespace Memstate.Test
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using Memstate.JsonNet;
    using Memstate.Models.KeyValue;

    [TestFixture]
    public class FileJournalTests
    {
        [Test]
        public async Task One_journal_entry_per_line_when_using_json_format()
        {
            const int NumRecords = 100;
            const string Stream = "test";
            const string FileName = Stream + ".journal";

            var settings = new MemstateSettings().WithInmemoryStorage();
            settings.Serializers.Register("newtonsoft.json", _ => new JsonSerializerAdapter(settings));
            settings.Serializer = "newtonsoft.json";

            settings.StreamName = Stream;
            var provider = settings.CreateStorageProvider();
            
            //Write NumRecords entries 
            var writer = provider.CreateJournalWriter(0);
            foreach (var i in Enumerable.Range(1, NumRecords))
            {
                writer.Send(new Set<int>("key" + i, i));
            }

            //wait for the writes to complete
            await writer.DisposeAsync().ConfigureAwait(false);

            //Read back all the entries, should be NumRecords
            var reader = provider.CreateJournalReader();
            Assert.AreEqual(NumRecords, reader.GetRecords().Count());
            await reader.DisposeAsync().ConfigureAwait(false);

            //Count the actual lines in the file
            Assert.IsTrue(settings.FileSystem.Exists(FileName));
            var streamReader = new StreamReader(settings.FileSystem.OpenRead(FileName));
            var lines = 0;
            while (true)
            {
                var line = streamReader.ReadLine();
                if (line == null) break;
                Console.WriteLine("> " + line);
                lines++;
            }
            Assert.AreEqual(NumRecords, lines);

        }
    }
}