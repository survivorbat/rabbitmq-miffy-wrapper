using System.Collections;
using System.Collections.Generic;
using ExampleMicroService.Models;
using Microsoft.EntityFrameworkCore;

namespace ExampleMicroService.DAL
{
    /// <summary>
    /// An example context, this is obviously NOT required to use
    /// the library.
    /// </summary>
    public class PolisContext : DbContext
    {
        /// <summary>
        /// Instantiate a new PolisContext with injected options
        /// </summary>
        /// <param name="options"></param>
        public PolisContext(DbContextOptions<PolisContext> options) : base(options) { }
        
        /// <summary>
        /// A list of Polissen
        /// </summary>
        public DbSet<Polis> Polissen { get; set; }
    }
}