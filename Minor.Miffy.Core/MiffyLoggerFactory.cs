using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Minor.Miffy
{
    public static class MiffyLoggerFactory
    {
        /// <summary>
        /// Initialize a loggerfactory field
        /// </summary>
        private static ILoggerFactory _loggerFactory = new NullLoggerFactory();
        
        public static ILoggerFactory LoggerFactory
        {
            internal get => _loggerFactory;
            set
            {
                if (_loggerFactory is NullLoggerFactory)
                {
                    _loggerFactory = value;
                }
                else 
                {
                    throw new InvalidOperationException("Loggerfactory has already been set");
                }
            }
        }
        
        internal static ILogger<T> CreateInstance<T>() => LoggerFactory.CreateLogger<T>();
    }
}