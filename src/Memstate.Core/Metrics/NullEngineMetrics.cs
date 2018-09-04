using System;

namespace Memstate
{
    public class NullEngineMetrics : IEngineMetrics
    {
        public IDisposable MeasureQueryExecution() => NullDisposable.Instance;

        public void QueryExecuted() { }

        public void QueryFailed() { }

        public void CommandExecuted() { }

        public void CommandFailed() { }

        public void PendingLocalCommands(int value) { }

        public IDisposable MeasureCommandExecution() => NullDisposable.Instance;
    }
}