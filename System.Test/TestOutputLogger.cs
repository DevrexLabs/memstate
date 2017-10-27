namespace System.Test
{
    using System;

    using Microsoft.Extensions.Logging;

    using Xunit.Abstractions;

    public class TestOutputLogger : ILogger
    {
        private readonly LogLevel _minimumLevel;
        private readonly string _category;
        private readonly ITestOutputHelper _testOutputHelper;

        public TestOutputLogger(LogLevel minimumLevel, string category, ITestOutputHelper testOutputHelper)
        {
            _minimumLevel = minimumLevel;
            _category = category;
            _testOutputHelper = testOutputHelper;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            const string Template = "{0} - {1} - {2} - {3}";

            if (logLevel >= _minimumLevel)
            {
                try
                {
                    _testOutputHelper.WriteLine(
                        Template,
                        DateTime.Now.ToString("O"),
                        logLevel.ToString(),
                        _category,
                        formatter.Invoke(state, exception));
                }
                catch
                {
                    // ignored
                }
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