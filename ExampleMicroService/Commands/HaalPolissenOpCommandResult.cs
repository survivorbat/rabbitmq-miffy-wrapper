using System.Collections.Generic;
using ExampleMicroService.Models;

namespace ExampleMicroService.Commands
{
    /// <summary>
    /// A dummy object that will be sent over the bus as a result
    /// </summary>
    public class HaalPolissenOpCommandResult
    {
        /// <summary>
        /// List of polissen that will be retrieved from the command queue
        /// </summary>
        public IEnumerable<Polis> Polissen { get; set; }
    }
}
