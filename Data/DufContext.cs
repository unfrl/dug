using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using dug.Data.Models;

namespace dug.Data
{
    public class DugContext : DbContext
    {
        public DbSet<DnsServer> DnsServers { get; set;}

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite($"Data Source={Config.SqliteFile}");

        protected override void OnModelCreating(ModelBuilder builder){
            builder.Entity<DnsServer>(entity => {
                entity.HasIndex(e => e.IPAddress).IsUnique();
            });
        }
    }
}