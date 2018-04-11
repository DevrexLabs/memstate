using System;
using System.Linq;
using System.Threading.Tasks;

namespace Memstate.Host.Commands
{
    public class BenchmarkCommand : ICommand
    {
        private readonly WriteBenchmarkCommand _writeBenchmark = new WriteBenchmarkCommand();

        private readonly ReadBenchmarkCommand _readBenchmark = new ReadBenchmarkCommand();

        public event EventHandler Done = (sender, args) => { };

        public async Task Start(string[] arguments)
        {
            await _writeBenchmark.Start(arguments.ToArray());
            await _readBenchmark.Start(arguments.ToArray());
        }

        public async Task Stop()
        {
            await _writeBenchmark.Stop();
            await _readBenchmark.Stop();
        }
    }
}