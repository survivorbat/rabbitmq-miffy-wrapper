using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Minor.Miffy
{
    public static class MiffyLoggerFactory
    {
        public static ILoggerFactory LoggerFactory { get; set; } = new NullLoggerFactory();
        public static ILogger<T> CreateInstance<T>() => LoggerFactory.CreateLogger<T>();
    }
}