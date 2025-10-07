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
        return _context.Trips.ToList();
    }

    public Trip? GetById(int id)
    {
        return _context.Trips.FirstOrDefault(t => t.Id == id);
    }

    public List<Trip> GetByVehicleId(int vehicleId)
    {
        return _context.Trips
            .Where(t => t.VehicleId == vehicleId)
            .ToList();
    }

    // 🔹 Trip hinzufügen (mit automatischer Berechnung der Dauer und Durchschnittsgeschwindigkeit)
    public Trip Add(Trip trip)
    {
        if (trip.EndTime < trip.StartTime)
        {
            throw new Exception("EndTime darf nicht vor StartTime liegen.");
        }

        double durationHours = (trip.EndTime - trip.StartTime).TotalHours;
        double avgSpeed = durationHours > 0 ? trip.DistanceKm / durationHours : 0;

        Console.WriteLine($"⏱️ Dauer: {durationHours:F2}h | Ø Geschwindigkeit: {avgSpeed:F1} km/h");

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
