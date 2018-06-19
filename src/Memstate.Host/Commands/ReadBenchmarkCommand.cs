using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Memstate.Models.KeyValue;
using Memstate.Logging;

namespace Memstate.Host.Commands
{
    public class ReadBenchmarkCommand : Benchmark
    {
        private const int Reads = 1000000;

        protected override int Runs => 10;

        protected override async Task Run()
        {
            await Engine.Execute(new Set<int>("key-0", 0));
            
            for (var i = 0; i < Reads; i++)
            {
                await Engine.Execute(new Get<int>("key-0"));
            }
        }

        protected override Task Total(TimeSpan total)
        {
            Logger.Info($"Read {Reads} queries in {total.TotalSeconds}s. {Reads / total.TotalSeconds:0.00} queries/second.");

            return Task.CompletedTask;
        }

        protected override async Task Totals(IEnumerable<TimeSpan> totals)
        {
            var queriesPerSeconds = totals.Select(total => Reads / total.TotalSeconds).ToArray();

            Logger.Info($"Min: {queriesPerSeconds.Min():0.00}");
            Logger.Info($"Max: {queriesPerSeconds.Max():0.00}");
            Logger.Info($"Average: {queriesPerSeconds.Average():0.00}");
            Logger.Info(await Metrics());
        }
    }
}