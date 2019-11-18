using System.Collections;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using VoorbeeldMicroService.Models;

namespace VoorbeeldMicroService.DAL
{
    public class PolisContext : DbContext
    {
        public PolisContext() {}
        public PolisContext(DbContextOptions<PolisContext> options) : base(options) { }
        
        public DbSet<Polis> Polissen { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql("Host=localhost;Database=test;Username=root;Password=root");
            }
        }
    }
}