using System.Linq;
using AutoMindBackend.Data;
using AutoMindBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoMindBackend.Services;

public class TripService
{
    private readonly AppDbContext _context;

    public TripService(AppDbContext context)
    {
        _context = context;
    }

    // Admin: alle Trips
    public List<TripDto> GetAll()
    {
        return _context.Trips
            .Include(t => t.Vehicle)
            .Include(t => t.User)
            .OrderByDescending(t => t.StartTime)
            .Select(t => t.ToDto())
            .ToList();
    }

    // Alle Trips eines Users
    public List<TripDto> GetAllByUserId(int userId)
    {
        return _context.Trips
            .Where(t => t.UserId == userId)
            .Include(t => t.Vehicle)
            .Include(t => t.User)
            .OrderByDescending(t => t.StartTime)
            .Select(t => t.ToDto())
            .ToList();
    }

    // Trip nach Id
    public TripDto? GetById(int id)
    {
        var trip = _context.Trips
            .Include(t => t.Vehicle)
            .Include(t => t.User)
            .FirstOrDefault(t => t.Id == id);

        return trip?.ToDto();
    }

    // Trip nach Id + UserId (damit User nur seine Trips sieht)
    public TripDto? GetByIdAndUserId(int id, int userId)
    {
        var trip = _context.Trips
            .Include(t => t.Vehicle)
            .Include(t => t.User)
            .FirstOrDefault(t => t.Id == id && t.UserId == userId);

        return trip?.ToDto();
    }

    // Trips nach VehicleId
    public List<TripDto> GetByVehicleId(int vehicleId)
    {
        return _context.Trips
            .Where(t => t.VehicleId == vehicleId)
            .Include(t => t.Vehicle)
            .Include(t => t.User)
            .OrderByDescending(t => t.StartTime)
            .Select(t => t.ToDto())
            .ToList();
    }

    // Trips eines Users nach VehicleId
    public List<TripDto> GetByVehicleIdAndUserId(int vehicleId, int userId)
    {
        return _context.Trips
            .Where(t => t.VehicleId == vehicleId && t.UserId == userId)
            .Include(t => t.Vehicle)
            .Include(t => t.User)
            .OrderByDescending(t => t.StartTime)
            .Select(t => t.ToDto())
            .ToList();
    }

    // Trip anlegen (wird z.B. vom Controller benutzt, oder DataSeeder)
    public Trip Add(Trip trip)
    {
        _context.Trips.Add(trip);
        _context.SaveChanges();
        return trip;
    }

    // Trip löschen
    public bool Delete(int id)
    {
        var trip = _context.Trips.Find(id);
        if (trip == null) return false;

        _context.Trips.Remove(trip);
        _context.SaveChanges();
        return true;
    }
}

// Mapping-Erweiterung: Trip -> TripDto
public static class TripMappingExtensions
{
    public static TripDto ToDto(this Trip trip)
    {
        return new TripDto
        {
            Id = trip.Id,
            StartTime = trip.StartTime,
            EndTime = trip.EndTime ?? trip.StartTime, // EndTime ist DateTime? im Model
            DistanceKm = trip.DistanceKm,
            StartLocation = trip.StartLocation,
            EndLocation = trip.EndLocation,
            VehicleId = trip.VehicleId
        };
    }
}
