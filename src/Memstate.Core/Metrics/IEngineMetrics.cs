using System;

namespace Memstate
{
    public interface IEngineMetrics
    {
        void QueryExecuted();
        void QueryFailed();
        void CommandExecuted();
        void CommandFailed();
        void PendingLocalCommands(int value);
        IDisposable MeasureCommandExecution();
        IDisposable MeasureQueryExecution();
    }
}