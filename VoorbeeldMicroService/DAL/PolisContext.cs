using System.Collections;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using VoorbeeldMicroService.Models;

namespace VoorbeeldMicroService.DAL
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

        /// <summary>
        /// Configure the database in case no options have been provided
        /// </summary>
        /// <param name="optionsBuilder"></param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql("Host=localhost;Database=test;Username=root;Password=root");
            }
        }
    }
}