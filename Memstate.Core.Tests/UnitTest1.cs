using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Memstate.JsonNet;
using Xunit;
using Xunit.Abstractions;

namespace Memstate.Core.Tests
{
    public class SmokeTests
    {
        private readonly ITestOutputHelper _log;

        public SmokeTests(ITestOutputHelper log)
        {
            _log = log;
        }

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

        [Fact]
        public void Test1()
        {
            var model = new List<String>();
            Kernel k = new Kernel(model);
            int numStrings = (int) k.Execute(new AddStringCommand(String.Empty));
            Assert.Equal(1,numStrings);
            _log.WriteLine("hello test");
        }

        [Fact]
        public void SmokeTest()
        {
            var initialModel = new List<string>();
            var commandStore = new InMemoryCommandStore();
            var engine = new Engine<List<string>>(initialModel,commandStore, commandStore, 1);
            var tasks = Enumerable.Range(10, 10000)
                .Select(n => engine.ExecuteAsync(new AddStringCommand(n.ToString())))
                .ToArray();

            int expected = 1;
            foreach(var task in tasks) Assert.Equal(expected++, task.Result);
        }

        [Fact]
        public void FileCommandStoreWorkout()
        {
            var fileName = Path.GetTempFileName();
            var serializer = new JsonSerializerAdapter();
            var store = new FileCommandStore(fileName, serializer);
            var records = new List<JournalRecord>();
            var sub = store.Subscribe(1, records.Add);
            for (int i = 0; i < 1000; i++)
            {
                var command = new AddStringCommand(i.ToString());
                store.Handle(command);
            }
            store.Dispose();
            sub.Dispose();
            Assert.Equal(1000, records.Count);

            //simulate replay, records are read from file
            records.Clear();
            store = new FileCommandStore(fileName,serializer);
            sub = store.Subscribe(1, records.Add);
            Assert.Equal(1000,records.Count);
            
            //push more commands
            for (int i = 0; i < 1000; i++)
            {
                var command = new AddStringCommand(i.ToString());
                store.Handle(command);
            }
            store.Dispose();
            sub.Dispose();
            Assert.Equal(2000,records.Count);
        }


    }
}
