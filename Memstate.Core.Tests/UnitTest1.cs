using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Memstate.Core.Tests
{
    public class UnitTest1
    {
        private ITestOutputHelper _log;

        public UnitTest1(ITestOutputHelper log)
        {
            _log = log;
        }

        class AddString : Command<List<string>, int>
        {
            public readonly string StringToAdd;

            public AddString(string stringToAdd)
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
            int numStrings = (int) k.Execute(new AddString(String.Empty));
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
                .Select(n => engine.ExecuteAsync(new AddString(n.ToString())))
                .ToArray();

            int expected = 1;
            foreach(var task in tasks) Assert.Equal(expected++, task.Result);
        }
    }
}
