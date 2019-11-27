using System;

namespace ExampleMicroService.Models
{
    /// <summary>
    /// An example model object that will be transmitted
    /// over the bus
    /// </summary>
    public class Polis
    {
        /// <summary>
        /// ID of the Polis
        /// </summary>
        public Guid Id { get; } = Guid.NewGuid();
        
        /// <summary>
        /// Name of the customer
        /// </summary>
        public string Klantnaam { get; set; }
    }
}