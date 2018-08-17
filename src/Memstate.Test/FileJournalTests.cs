using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Memstate.Configuration;
using Memstate.Models.KeyValue;

namespace Memstate.Test
{
    [TestFixture]
    public class FileJournalTests
    {
        [Test]
        public async Task One_journal_entry_per_line_when_using_json_format()
        {
            const int NumRecords = 100;
            const string Stream = "test";
            const string FileName = Stream + ".journal";

            var cfg = Config.Reset();
            cfg.UseInMemoryFileSystem();
            var settings = cfg.Resolve<MemstateSettings>();
            cfg.SerializerName = "newtonsoft.json";

            settings.StreamName = Stream;
            var provider = cfg.GetStorageProvider();
            
            //Write NumRecords entries 
            var writer = provider.CreateJournalWriter(0);
            foreach (var i in Enumerable.Range(1, NumRecords))
            {
                writer.Send(new Set<int>("key" + i, i));
            }

            //wait for the writes to complete
            await writer.DisposeAsync().ConfigureAwait(false);

            //Get back all the entries, should be NumRecords
            var reader = provider.CreateJournalReader();
            Assert.AreEqual(NumRecords, reader.GetRecords().Count());
            await reader.DisposeAsync().ConfigureAwait(false);

            //Count the actual lines in the file
            Assert.IsTrue(cfg.FileSystem.Exists(FileName));
            var streamReader = new StreamReader(cfg.FileSystem.OpenRead(FileName));
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