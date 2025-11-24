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

    public List<Trip> GetAll()
    {
        return _context.Trips
            .Include(t => t.User) // ✅ Jetzt funktioniert es!
            .Include(t => t.Vehicle)
            .ToList();
    }

    public List<Trip> GetAllByKeycloakUser(string keycloakUserId)
    {
        return _context.Trips
            .Include(t => t.User)
            .Include(t => t.Vehicle)
            .Where(t => t.User.KeycloakId == keycloakUserId)
            .ToList();
    }

    public Trip GetById(int id)
    {
        return _context.Trips
            .Include(t => t.User)
            .Include(t => t.Vehicle)
            .FirstOrDefault(t => t.Id == id);
    }

    public Trip GetByIdAndKeycloakUser(int id, string keycloakUserId)
    {
        return _context.Trips
            .Include(t => t.User)
            .Include(t => t.Vehicle)
            .FirstOrDefault(t => t.Id == id && t.User.KeycloakId == keycloakUserId);
    }

    public List<Trip> GetByVehicleId(int vehicleId)
    {
        return _context.Trips
            .Include(t => t.User)
            .Include(t => t.Vehicle)
            .Where(t => t.VehicleId == vehicleId)
            .ToList();
    }

    public List<Trip> GetByVehicleIdAndKeycloakUser(int vehicleId, string keycloakUserId)
    {
        return _context.Trips
            .Include(t => t.User)
            .Include(t => t.Vehicle)
            .Where(t => t.VehicleId == vehicleId && t.User.KeycloakId == keycloakUserId)
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