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
            const int numRecords = 100;
            const string stream = "test";
            const string fileName = stream + ".journal";

            var cfg = Config.CreateDefault();
            var settings = cfg.GetSettings<EngineSettings>();
            cfg.SerializerName = "newtonsoft.json";

            settings.StreamName = stream;
            var provider = cfg.GetStorageProvider();
            
            //Write NumRecords entries 
            var writer = provider.CreateJournalWriter();
            foreach (var i in Enumerable.Range(1, numRecords))
            {
                await writer.Write(new Set<int>("key" + i, i));
            }

            if (writer is FileJournalWriter fjw) 
                await fjw.StartWritingFrom(numRecords);

            //wait for the writes to complete
            await writer.DisposeAsync();

            //Get back all the entries, should be NumRecords
            var reader = provider.CreateJournalReader();
            Assert.AreEqual(numRecords, reader.ReadRecords().Count());
            await reader.DisposeAsync();

            //Count the actual lines in the file
            Assert.IsTrue(cfg.FileSystem.Exists(fileName));
            var streamReader = new StreamReader(cfg.FileSystem.OpenRead(fileName));
            var lines = 0;
            while (true)
            {
                var line = await streamReader.ReadLineAsync();
                if (line == null) break;
                Console.WriteLine("> " + line);
                lines++;
            }
            Assert.AreEqual(numRecords, lines);

        }
    }
}