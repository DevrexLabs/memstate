using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Memstate.JsonNet;
using Xunit;
using Xunit.Abstractions;

namespace Memstate.Tests
{
    class AddStringCommand : Command<List<string>, int>
    {
        public readonly string StringToAdd;

        public AddStringCommand(string stringToAdd)
        {
            StringToAdd = stringToAdd;
        }

        public override int Execute(List<string> model)
        {
            model.Add(StringToAdd);
            return model.Count;
        }
    }
    public class CommandTests
    {

        public static IEnumerable<object[]> Serializers()
        {
            yield return new object[] {new JsonSerializerAdapter()};
        } 

        [MemberData(nameof(Serializers))]
        [Theory]
        public void Command_keeps_id_after_serialization(ISerializer serializer)
        {
            var original = new AddStringCommand("dummy");
            var clone = serializer.Clone(original);
            Assert.Equal(original.Id, clone.Id);
        }
    }
    public class SmokeTests
    {
        private readonly ITestOutputHelper _log;

        public SmokeTests(ITestOutputHelper log)
        {
            _log = log;
        }
        [Fact]
        public void Test1()
        {
            var config = new Settings();
            var model = new List<String>();
            Kernel k = new Kernel(config,model);
            int numStrings = (int) k.Execute(new AddStringCommand(String.Empty));
            Assert.Equal(1,numStrings);
            _log.WriteLine("hello test");
        }

        [Fact]
        public void SmokeTest()
        {
            var config = new Settings();
            var initialModel = new List<string>();
            var commandStore = new InMemoryCommandStore(config);
            var engine = new Engine<List<string>>(config, initialModel,commandStore, commandStore, 0);
            var tasks = Enumerable.Range(10, 10000)
                .Select(n => engine.ExecuteAsync(new AddStringCommand(n.ToString())))
                .ToArray();

            int expected = 1;
            foreach(var task in tasks) Assert.Equal(expected++, task.Result);
        }

        [Fact]
        public void FileJournalSmokeTest()
        {
            var config = new Settings();
            var fileName = Path.GetTempFileName();
            var serializer = new JsonSerializerAdapter();
            var journalWriter = new FileJournalWriter(config, serializer, fileName, 0);
            var subSource = new FileJournalSubscriptionSource(journalWriter);
            var records = new List<JournalRecord>();
            var sub = subSource.Subscribe(0, records.Add);
            for (int i = 0; i < 1000; i++)
            {
                var command = new AddStringCommand(i.ToString());
                journalWriter.Send(command);
            }
            journalWriter.Dispose();
            sub.Dispose();
            subSource.Dispose();

            Assert.Equal(1000, records.Count);

           

            var reader = new FileJournalReader(fileName, serializer);
            records.Clear();
            foreach (var record in reader.GetRecords())
            {
                records.Add(record);
            }
            Assert.True(records.Select(r => (int)r.RecordNumber).SequenceEqual(Enumerable.Range(0, 1000)));
            reader.Dispose();
            File.Delete(fileName);
        }
    }
}
