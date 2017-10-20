using System;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace EventStore.Tests
{
    public class TestOutputLoggingProvider : ILoggerProvider
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public void Dispose()
        {
        }

        public TestOutputLoggingProvider(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new TestOutputLogger(MinimumLogLevel, categoryName, _testOutputHelper);
        }

        public LogLevel MinimumLogLevel { get; set; } = LogLevel.Trace;
    }
}