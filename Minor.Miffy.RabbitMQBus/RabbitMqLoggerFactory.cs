using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Minor.Miffy.RabbitMQBus
{
    public static class RabbitMqLoggerFactory
    {
        /// <summary>
        /// Logger factory defaults to Null to prevent errors from popping up
        /// </summary>
        private static ILoggerFactory _loggerFactory = new NullLoggerFactory();

        /// <summary>
        /// The loggerfactory can only be used internally and can
        /// only be set once.
        /// </summary>
        public static ILoggerFactory LoggerFactory
        {
            internal get => _loggerFactory;
            set
            {
                if (LoggerFactory is NullLoggerFactory)
                {
                    _loggerFactory = value;
                }
                else
                {
                    throw new InvalidOperationException("Loggerfactory has already been set");
                }
            }
        }

        /// <summary>
        /// Create a logger instance from the logger factory
        /// </summary>
        internal static ILogger<T> CreateInstance<T>() => LoggerFactory.CreateLogger<T>();
    }
}
