using System;

namespace Minor.Miffy
{
    /// <summary>
    /// Special commandmessage with an exception
    /// </summary>
    public class CommandError : CommandMessage
    {
        /// <summary>
        /// TODO: Replace exception type with something more specific
        /// 
        /// Exception thrown
        /// </summary>
        public Exception Exception { get; set; }
    }
}