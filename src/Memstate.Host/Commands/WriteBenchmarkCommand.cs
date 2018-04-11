using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Memstate.Models.KeyValue;
using Microsoft.Extensions.Logging;

namespace Memstate.Host.Commands
{
    public class WriteBenchmarkCommand : Benchmark
    {
        private const int Writes = 10000;

        protected override int Runs => 10;

        protected override Task Run()
        {
            var tasks = new Task[Writes];

            for (var i = 0; i < Writes; i++)
            {
                tasks[i] = Engine.ExecuteAsync(new Set<int>($"key-{i}", i));
            }

            Task.WaitAll(tasks);

            return Task.CompletedTask;
        }

        protected override Task Total(TimeSpan total)
        {
            Logger.LogInformation($"Executed {Writes} commands in {total.TotalSeconds}s. {Writes / total.TotalSeconds:0.00} commands/second");

            return Task.CompletedTask;
        }

        protected override async Task Totals(IEnumerable<TimeSpan> totals)
        {
            var commandsPerSeconds = totals.Select(total => Writes / total.TotalSeconds).ToArray();

            Logger.LogInformation($"Min: {commandsPerSeconds.Min():0.00}");
            Logger.LogInformation($"Max: {commandsPerSeconds.Max():0.00}");
            Logger.LogInformation($"Average: {commandsPerSeconds.Average():0.00}");
            Logger.LogInformation(await Metrics());
        }
    }
}