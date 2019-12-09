using System;

namespace Minor.Miffy
{
    /// <summary>
    /// Exception to throw if configuration is invalid
    /// </summary>
    public class BusConfigurationException : Exception
    {
        /// <summary>
        /// Create a simple exception instance
        /// </summary>
        public BusConfigurationException() { }

        /// <summary>
        /// Create an exception instance with a message
        /// </summary>
        /// <param name="message">Message to highlight</param>
        public BusConfigurationException(string message) : base(message) { }

        /// <summary>
        /// Create a busconfigurationexception with an inner exception
        /// </summary>
        /// <param name="message">Message to highlight</param>
        /// <param name="innerException">InnerException</param>
        public BusConfigurationException(string message, Exception innerException) : base(message, innerException) { }
    }
}
