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
    
    public double GetTotalFuelUsed(int vehicleId)
    {
        return _context.Trips
            .Where(t => t.VehicleId == vehicleId)
            .Sum(t => t.FuelUsed);
    }

    public double GetTotalCost(int vehicleId)
    {
        return _context.Trips
            .Where(t => t.VehicleId == vehicleId)
            .Sum(t => t.TripCost);
    }

    public double GetAverageConsumption(int vehicleId)
    {
        var totalKm = GetTotalDistance(vehicleId);
        var totalFuel = GetTotalFuelUsed(vehicleId);
        return totalKm > 0 ? (totalFuel / totalKm) * 100 : 0;
    }

    public bool Delete(int id)
    {
        var trip = _context.Trips.Find(id);
        if (trip == null) return false;

        _context.Trips.Remove(trip);
        _context.SaveChanges();
        return true;
    }
    public List<Trip> GetAllByUser(string username)
    {
        var user = _context.Users.FirstOrDefault(u => u.Username == username);
        if (user == null) return new List<Trip>();

        return _context.Trips
            .Where(t => t.UserId == user.Id)
            .ToList();
    }

    public Trip? GetByIdAndUser(int id, string username)
    {
        var user = _context.Users.FirstOrDefault(u => u.Username == username);
        if (user == null) return null;

        return _context.Trips.FirstOrDefault(t => t.Id == id && t.UserId == user.Id);
    }

    public List<Trip> GetByVehicleIdAndUser(int vehicleId, string username)
    {
        var user = _context.Users.FirstOrDefault(u => u.Username == username);
        if (user == null) return new List<Trip>();

        return _context.Trips
            .Where(t => t.VehicleId == vehicleId && t.UserId == user.Id)
            .ToList();
    }

    public bool DeleteByUser(int id, string username)
    {
        var user = _context.Users.FirstOrDefault(u => u.Username == username);
        if (user == null) return false;

        var trip = _context.Trips.FirstOrDefault(t => t.Id == id && t.UserId == user.Id);
        if (trip == null) return false;

        _context.Trips.Remove(trip);
        _context.SaveChanges();
        return true;
    }

    public Trip AddForUser(Trip trip, string username, string role)
    {
        var currentUser = _context.Users.FirstOrDefault(u => u.Username == username);
        if (currentUser == null)
            throw new Exception("Benutzer nicht gefunden.");

        if (role == "Admin")
        {
            if (trip.UserId == 0)
            {
                trip.UserId = currentUser.Id;
            }
            else
            {
                var targetUser = _context.Users.FirstOrDefault(u => u.Id == trip.UserId);
                if (targetUser == null)
                    throw new Exception($"Benutzer mit ID {trip.UserId} existiert nicht.");
            }
        }
        else
        {
            trip.UserId = currentUser.Id;

            var vehicle = _context.Vehicles.FirstOrDefault(v => v.Id == trip.VehicleId && v.UserId == currentUser.Id);
            if (vehicle == null)
                throw new Exception("Kein Zugriff auf dieses Fahrzeug!");
        }

        var usedVehicle = _context.Vehicles.FirstOrDefault(v => v.Id == trip.VehicleId);
        if (usedVehicle == null)
            throw new Exception("Fahrzeug nicht gefunden.");

        double durationHours = (trip.EndTime - trip.StartTime).TotalHours;
        trip.AverageSpeed = durationHours > 0 ? trip.DistanceKm / durationHours : 0;
        trip.FuelUsed = trip.DistanceKm * (usedVehicle.FuelConsumption / 100);
        trip.TripCost = trip.FuelUsed * 1.80;

        _context.Trips.Add(trip);
        _context.SaveChanges();

        return trip;
    }

}
