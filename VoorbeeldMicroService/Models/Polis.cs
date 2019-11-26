namespace VoorbeeldMicroService.Models
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
        public long Id { get; set; }
        
        /// <summary>
        /// Name of the customer
        /// </summary>
        public string Klantnaam { get; set; }
    }
}