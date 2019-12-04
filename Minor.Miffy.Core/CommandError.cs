using System;

namespace Minor.Miffy
{
    /// <summary>
    /// Special commandmessage with an exception
    /// </summary>
    public class CommandError : CommandMessage
    {
        /// <summary>
        /// Exception thrown
        /// </summary>
        public Exception Exception { get; set; }
    }
}
