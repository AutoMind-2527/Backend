using Microsoft.EntityFrameworkCore;
using AutoMindBackend.Models;

namespace AutoMindBackend.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
     public DbSet<Trip> Trips => Set<Trip>();
}
