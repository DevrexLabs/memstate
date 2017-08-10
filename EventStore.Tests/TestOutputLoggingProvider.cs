using System;
using Memstate.Core;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace EventStore.Tests
{
    public class TestOutputLoggingProvider : ILoggerProvider
    {
        private static ITestOutputHelper _testOutputHelper;
        private static int _testContext = 0;
        public static void SetOutputHelper(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            _testContext++;
        }

        static TestOutputLoggingProvider()
        {
            Logging.Factory.AddProvider(new TestOutputLoggingProvider());

        }

        public void Dispose()
        {
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new TestOutputLogger(MinimumLogLevel, categoryName, _testOutputHelper, _testContext);
        }

        public LogLevel MinimumLogLevel { get; set; } = LogLevel.Trace;
    }
}