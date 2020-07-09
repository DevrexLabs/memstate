using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Memstate.Models;
using Memstate.Logging;
using Memstate.Configuration;

namespace Memstate.Runner.Commands
{
    public abstract class Benchmark : ICommand
    {
        public event EventHandler Done = (sender, args) => { };
        
        protected EngineSettings Settings { get; private set; }

        internal ILog Logger { get; private set; }

        protected Engine<KeyValueStore<int>> TheEngine { get; private set; }

        protected abstract int Runs { get; }

        public async Task Start(string[] arguments)
        {
            Settings = Program.Config.GetSettings<EngineSettings>();
            Settings.WithRandomSuffixAppendedToStreamName();

            Logger = LogProvider.GetCurrentClassLogger();

            TheEngine = await Engine.Start<KeyValueStore<int>>().NotOnCapturedContext(); 
            

            var totals = new List<TimeSpan>(Runs);

            for (var run = 0; run < Runs; run++)
            {
                Logger.Info($"Run {run + 1}");

                var stopwatch = Stopwatch.StartNew();

                await Run().NotOnCapturedContext();

                stopwatch.Stop();

                totals.Add(stopwatch.Elapsed);

                await Total(stopwatch.Elapsed);
            }

            await TheEngine.DisposeAsync();

            await Totals(totals);
            
            Done?.Invoke(this, EventArgs.Empty);
        }

        public Task Stop()
        {
            return Task.CompletedTask;
        }

        protected Task<string> GetMetrics()
        {
            return Metrics.Report();
        }

        protected abstract Task Run();

        protected virtual Task Total(TimeSpan total)
        {
            return Task.CompletedTask;
        }

        protected virtual Task Totals(IEnumerable<TimeSpan> totals)
        {
            return Task.CompletedTask;
        }
    }
}