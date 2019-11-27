using System.Collections.Generic;
using ExampleMicroService.Models;

namespace ExampleMicroService.DAL
{
    /// <summary>
    /// An example context, this is obviously NOT required to use
    /// the library.
    /// </summary>
    public class PolisContext
    {
        /// <summary>
        /// A list of Polissen
        /// </summary>
        public List<Polis> Polissen { get; } = new List<Polis>();
    }
}