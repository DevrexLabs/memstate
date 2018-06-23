using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Memstate.Models;
using Memstate.Logging;

namespace Memstate.Host.Commands
{
    public abstract class Benchmark : ICommand
    {
        public event EventHandler Done = (sender, args) => { };
        
        protected MemstateSettings Settings { get; private set; }

        internal ILog Logger { get; private set; }

        protected Engine<KeyValueStore<int>> Engine { get; private set; }

        protected abstract int Runs { get; }

        public async Task Start(string[] arguments)
        {
            var builder = new MsConfigSettingsBuilder(arguments);
            Settings = builder.Build<MemstateSettings>();
            Settings.WithRandomSuffixAppendedToStreamName();
            //Settings.LoggerFactory.AddConsole((category, level) => level > LogLevel.Debug);

            Logger = LogProvider.GetCurrentClassLogger();

            Engine = await new EngineBuilder(Settings).Build<KeyValueStore<int>>();

            var totals = new List<TimeSpan>(Runs);

            for (var run = 0; run < Runs; run++)
            {
                Logger.Info($"Run {run + 1}");

                var stopwatch = Stopwatch.StartNew();

                await Run();

                stopwatch.Stop();

                totals.Add(stopwatch.Elapsed);

                await Total(stopwatch.Elapsed);
            }

            await Engine.DisposeAsync();

            await Totals(totals);
            
            Done?.Invoke(this, EventArgs.Empty);
        }

        public Task Stop()
        {
            return Task.CompletedTask;
        }

        protected async Task<string> Metrics()
        {
            var snapshot = Settings.Metrics.Snapshot.Get();
            var formatter = Settings.Metrics.DefaultOutputMetricsFormatter;

            using (var stream = new MemoryStream())
            {
                await formatter.WriteAsync(stream, snapshot);

                var result = Encoding.UTF8.GetString(stream.ToArray());

                return result;
            }
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