using Memstate.Core;
using Xunit.Abstractions;

namespace EventStore.Tests
{
    public abstract class MemstateTestBase
    {
        protected readonly Config Config;
        protected readonly ITestOutputHelper Log;

        protected MemstateTestBase(ITestOutputHelper testOutputHelper)
        {
            Log = testOutputHelper;
            Config = new Config();
            Config.LoggerFactory.AddProvider(new TestOutputLoggingProvider(testOutputHelper));
        }
    }
}