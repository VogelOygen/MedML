using Microsoft.EntityFrameworkCore;
using MedML.Models;

namespace MedML.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<HeartDiseaseRecord> HeartDiseaseRecords { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Assuming local PostgreSQL setup. 
            // In a real app, this should be in a configuration file.
            optionsBuilder.UseNpgsql("Host=localhost;Database=MedML;Username=postgres;Password=longpassjkd");
        }
    }
}
