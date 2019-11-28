using System;
using ExampleMicroService.Commands;
using ExampleMicroService.Exceptions;
using Minor.Miffy.MicroServices.Events;

namespace ExampleMicroService.EventListeners
{
    /// <summary>
    /// An example command listener that throws an exception to
    /// demonstrate how to catch external exceptions in Program.cs
    /// </summary>
    [CommandListener("exception.test")]
    public class ExceptionCommandListener
    {
        /// <summary>
        /// Handle a Command and immediately throw an exception without any reason
        /// </summary>
        /// <param name="polissenOpCommand">Command</param>
        /// <returns>An exception</returns>
        public ExceptionCommand Handles(ExceptionCommand polissenOpCommand)
        {
            throw new MysteriousException("Something mysterious happened!");
        }
    }
}