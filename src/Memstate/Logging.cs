using Microsoft.Extensions.Logging;

namespace Memstate.Core
{
    public static class Logging
    {
        public static ILoggerFactory Factory { get; } = new LoggerFactory();
        public static ILogger CreateLogger<T>() => LoggerFactoryExtensions.CreateLogger<T>(Factory);
    }
}