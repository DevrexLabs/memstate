using System;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace EventStore.Tests
{
    public class TestOutputLogger : ILogger
    {
        private readonly LogLevel _minimumLevel;
        private readonly string _category;
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly int _testContext;

        public TestOutputLogger(LogLevel minimumLevel, string category, ITestOutputHelper testOutputHelper, int testContext)
        {
            _minimumLevel = minimumLevel;
            _category = category;
            _testOutputHelper = testOutputHelper;
            _testContext = testContext;
        }
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            const string template = "{0} - {1} - {2} - {3} - {4}";

            if (logLevel >= _minimumLevel)
            {
                _testOutputHelper.WriteLine(template, DateTime.Now.ToString("O"), _testContext, logLevel.ToString(), _category, formatter.Invoke(state, exception));   
            }
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= _minimumLevel;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return NullDisposable.Default;
        }
    }
}