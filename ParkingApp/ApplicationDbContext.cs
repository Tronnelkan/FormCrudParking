// ApplicationDbContext.cs

using Microsoft.EntityFrameworkCore;
using System.Configuration;

namespace BusManagementApp
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<Bus> Buses { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Отримання рядка підключення з App.config
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            optionsBuilder.UseNpgsql(connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Налаштування первинних ключів та обмежень для Driver
            modelBuilder.Entity<Driver>(entity =>
            {
                entity.HasKey(e => e.IdDriver);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.CategoryDriverLicence).IsRequired().HasMaxLength(10);
            });

            // Налаштування первинних ключів, унікальності та зв'язків для Bus
            modelBuilder.Entity<Bus>(entity =>
            {
                entity.HasKey(e => e.IdBus);
                entity.Property(e => e.BusMark).IsRequired().HasMaxLength(50);
                entity.Property(e => e.NumberSign).IsRequired().HasMaxLength(20);
                entity.HasIndex(e => e.NumberSign).IsUnique();

                // Визначення зв'язку між Bus і Driver
                entity.HasOne(e => e.Driver)
                      .WithMany(d => d.Buses)
                      .HasForeignKey(e => e.TheDriver)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
