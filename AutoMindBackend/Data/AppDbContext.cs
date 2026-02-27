using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using AutoMindBackend.Models;
using AutoMindBackend.Services;


namespace AutoMindBackend.Data;

public class AppDbContext : DbContext
{
    private readonly IAuditUserProvider? _auditUserProvider;

    public AppDbContext(DbContextOptions<AppDbContext> options, IAuditUserProvider? auditUserProvider = null)
        : base(options)
    {
        _auditUserProvider = auditUserProvider;
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<Trip> Trips { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var auditEntries = CreateAuditEntries();

        // 1) normale Änderungen speichern
        var result = await base.SaveChangesAsync(cancellationToken);

        // 2) danach audit logs speichern (damit IDs etc. schon existieren)
        if (auditEntries.Count > 0)
        {
            AuditLogs.AddRange(auditEntries);
            await base.SaveChangesAsync(cancellationToken);
        }

        return result;
    }

    private List<AuditLog> CreateAuditEntries()
    {
        ChangeTracker.DetectChanges();

        var audits = new List<AuditLog>();

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is AuditLog) continue;
            if (entry.State is EntityState.Detached or EntityState.Unchanged) continue;

            var audit = new AuditLog
            {
                EntityName = entry.Metadata.ClrType.Name,
                Action = entry.State.ToString(),
                Timestamp = DateTime.UtcNow,
                UserId = _auditUserProvider?.GetUserId(),
                UserName = _auditUserProvider?.GetUserName(),
                IpAddress = _auditUserProvider?.GetIpAddress()
            };

            if (entry.State == EntityState.Added)
            {
                audit.NewValues = JsonSerializer.Serialize(entry.CurrentValues.ToObject());
            }
            else if (entry.State == EntityState.Deleted)
            {
                audit.OldValues = JsonSerializer.Serialize(entry.OriginalValues.ToObject());
            }
            else if (entry.State == EntityState.Modified)
            {
                audit.OldValues = JsonSerializer.Serialize(entry.OriginalValues.ToObject());
                audit.NewValues = JsonSerializer.Serialize(entry.CurrentValues.ToObject());
            }

            audits.Add(audit);
        }

        return audits;
    }


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
            entity.Property(t => t.EndTime).IsRequired(false);

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