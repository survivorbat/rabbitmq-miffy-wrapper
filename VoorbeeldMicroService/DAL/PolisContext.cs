using System.Collections;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using VoorbeeldMicroService.Models;

namespace VoorbeeldMicroService.DAL
{
    public class PolisContext : DbContext
    {
        public PolisContext(DbContextOptions<PolisContext> options) : base(options) { }
        
        public ICollection<Polis> Polissen { get; set; } = new List<Polis>();
    }
}