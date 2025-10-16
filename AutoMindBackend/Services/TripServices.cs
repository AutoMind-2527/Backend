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

    public Trip Add(Trip trip)
    {
        var vehicle = _context.Vehicles.Find(trip.VehicleId);
        if (vehicle == null)
            throw new Exception("Fahrzeug nicht gefunden.");

        double durationHours = (trip.EndTime - trip.StartTime).TotalHours;
        trip.AverageSpeed = durationHours > 0 ? trip.DistanceKm / durationHours : 0;

        //Verbrauch
        trip.FuelUsed = trip.DistanceKm * (vehicle.FuelConsumption / 100);

        //1.80€ pro Liter
        double fuelPrice = 1.80;
        trip.TripCost = trip.FuelUsed * fuelPrice;

        _context.Trips.Add(trip);
        _context.SaveChanges();

        Console.WriteLine($"Trip gespeichert: {trip.DistanceKm:F1} km, {trip.FuelUsed:F2} L, {trip.TripCost:F2} €");
        return trip;
    }
    
    public double GetTotalDistance(int vehicleId)
    {
        return _context.Trips
            .Where(t => t.VehicleId == vehicleId)
            .Sum(t => t.DistanceKm);
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
