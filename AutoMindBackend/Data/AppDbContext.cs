using Microsoft.EntityFrameworkCore;
using AutoMindBackend.Models;

namespace AutoMindBackend.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // DbSets
    public DbSet<User> Users { get; set; }
    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<Trip> Trips { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User Configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Username).IsRequired().HasMaxLength(100);
            entity.Property(u => u.Email).HasMaxLength(255);
            entity.Property(u => u.Role).HasMaxLength(50).HasDefaultValue("User");
            entity.Property(u => u.KeycloakId).HasMaxLength(255);
            
            // Index für KeycloakId
            entity.HasIndex(u => u.KeycloakId).IsUnique();
            
            // Index für Username
            entity.HasIndex(u => u.Username).IsUnique();
        });

        // Vehicle Configuration
        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.HasKey(v => v.Id);
            entity.Property(v => v.LicensePlate).IsRequired().HasMaxLength(20);
            entity.Property(v => v.Brand).IsRequired().HasMaxLength(50);
            entity.Property(v => v.Model).IsRequired().HasMaxLength(50);
            entity.Property(v => v.Mileage).IsRequired();
            entity.Property(v => v.FuelConsumption).IsRequired().HasColumnType("decimal(5,2)");

            // Relationship mit User
            entity.HasOne(v => v.User)
                  .WithMany(u => u.Vehicles)
                  .HasForeignKey(v => v.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Trip Configuration
        modelBuilder.Entity<Trip>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.StartLocation).IsRequired().HasMaxLength(255);
            entity.Property(t => t.EndLocation).IsRequired().HasMaxLength(255);
            entity.Property(t => t.DistanceKm).IsRequired().HasColumnType("decimal(10,2)");
            entity.Property(t => t.StartTime).IsRequired();
            entity.Property(t => t.EndTime).IsRequired();

            // Relationship mit User
            entity.HasOne(t => t.User)
                  .WithMany(u => u.Trips)
                  .HasForeignKey(t => t.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Relationship mit Vehicle
            entity.HasOne(t => t.Vehicle)
                  .WithMany(v => v.Trips)
                  .HasForeignKey(t => t.VehicleId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Seed Data (optional)
        modelBuilder.Entity<User>().HasData(
            new User 
            { 
                Id = 1, 
                Username = "admin", 
                Email = "admin@automind.com",
                Role = "Admin",
                KeycloakId = "admin-keycloak-id"
            }
        );
    }
}