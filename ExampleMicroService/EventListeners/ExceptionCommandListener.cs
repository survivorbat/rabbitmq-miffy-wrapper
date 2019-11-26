using System;
using ExampleMicroService.Commands;
using Minor.Miffy.MicroServices.Events;

namespace ExampleMicroService.EventListeners
{
    /// <summary>
    /// An example command listener that throws an exception to
    /// demonstrate how to catch external exceptions in Program.cs
    /// </summary>
    [CommandListener("exception.listener")]
    public class ExceptionCommandListener
    {
        /// <summary>
        /// Handle a HaalPolissenOpCommand and immediately throw an exception without any reason
        /// </summary>
        /// <param name="polissenOpCommand">Command</param>
        /// <returns>An exception</returns>
        public HaalPolissenOpCommand Handles(HaalPolissenOpCommand polissenOpCommand)
        {
            throw new Exception("Something mysterious happened!");
        }
    }
}