using System;

namespace ExampleMicroService.Exceptions
{
    /// <summary>
    /// Exceptinn for testing
    /// </summary>
    [Serializable]
    public class MysteriousException : Exception
    {
        /// <summary>
        /// Exception without message
        /// </summary>
        public MysteriousException()
        {
            // Instantiate an extra mysterious exception
        }

        /// <summary>
        /// Exception with message
        /// </summary>
        public MysteriousException(string message) : base(message)
        {
            // Instantiate a mysterious exception with a message
        }
    }
}