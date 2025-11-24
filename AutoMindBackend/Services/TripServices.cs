using Microsoft.EntityFrameworkCore;
using AutoMindBackend.Data;
using AutoMindBackend.Models;

namespace AutoMindBackend.Services;

public class TripService
{
    private readonly AppDbContext _context;

    public TripService(AppDbContext context)
    {
        _context = context;
    }

    // Admin: alle Trips
    public List<Trip> GetAll()
    {
        return _context.Trips
            .Include(t => t.User)
            .Include(t => t.Vehicle)
            .ToList();
    }

    // User: alle eigenen Trips über UserId
    public List<Trip> GetAllByUserId(int userId)
    {
        return _context.Trips
            .Include(t => t.User)
            .Include(t => t.Vehicle)
            .Where(t => t.UserId == userId)
            .ToList();
    }

    public Trip? GetById(int id)
    {
        return _context.Trips
            .Include(t => t.User)
            .Include(t => t.Vehicle)
            .FirstOrDefault(t => t.Id == id);
    }

    public Trip? GetByIdAndUserId(int id, int userId)
    {
        return _context.Trips
            .Include(t => t.User)
            .Include(t => t.Vehicle)
            .FirstOrDefault(t => t.Id == id && t.UserId == userId);
    }

    public List<Trip> GetByVehicleId(int vehicleId)
    {
        return _context.Trips
            .Include(t => t.User)
            .Include(t => t.Vehicle)
            .Where(t => t.VehicleId == vehicleId)
            .ToList();
    }

    public List<Trip> GetByVehicleIdAndUserId(int vehicleId, int userId)
    {
        return _context.Trips
            .Include(t => t.User)
            .Include(t => t.Vehicle)
            .Where(t => t.VehicleId == vehicleId && t.UserId == userId)
            .ToList();
    }

    public Trip Add(Trip trip)
    {
        _context.Trips.Add(trip);
        _context.SaveChanges();
        return trip;
    }

    public bool Delete(int id)
    {
        var trip = _context.Trips.Find(id);
        if (trip == null) return false;

        _context.Trips.Remove(trip);
        _context.SaveChanges();
        return true;
    }
}
