namespace System.Test
{
    using Memstate;
    using Xunit.Abstractions;

    public abstract class MemstateTestBase
    {
        protected readonly MemstateSettings Config;
        protected readonly ITestOutputHelper Log;

        protected MemstateTestBase(ITestOutputHelper testOutputHelper)
        {
            Log = testOutputHelper;
            Config = new MemstateSettings();
            Config.LoggerFactory.AddProvider(new TestOutputLoggingProvider(testOutputHelper));
        }
    }
}