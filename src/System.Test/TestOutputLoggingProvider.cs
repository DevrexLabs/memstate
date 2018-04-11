namespace System.Test
{
    using Microsoft.Extensions.Logging;

    using Xunit.Abstractions;

    public class TestOutputLoggingProvider : ILoggerProvider
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public TestOutputLoggingProvider(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        public LogLevel MinimumLogLevel { get; set; } = LogLevel.Trace;

        public ILogger CreateLogger(string categoryName)
        {
            return new TestOutputLogger(MinimumLogLevel, categoryName, _testOutputHelper);
        }

        public void Dispose()
        {
        }

    }
}