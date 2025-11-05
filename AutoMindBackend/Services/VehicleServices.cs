using AutoMindBackend.Data;
using AutoMindBackend.Models;

namespace AutoMindBackend.Services;

public class VehicleService
{
    private readonly AppDbContext _context;

    public VehicleService(AppDbContext context)
    {
        _context = context;
    }

    public List<Vehicle> GetAll()
    {
        return _context.Vehicles.ToList();
    }

    public Vehicle? GetById(int id)
    {
        return _context.Vehicles.Find(id);
    }

    public Vehicle Add(Vehicle vehicle)
    {
        _context.Vehicles.Add(vehicle);
        _context.SaveChanges();
        return vehicle;
    }

    public bool Delete(int id)
    {
        var vehicle = _context.Vehicles.Find(id);
        if (vehicle == null) return false;

        _context.Vehicles.Remove(vehicle);
        _context.SaveChanges();
        return true;
    }

    public void AddMileage(int vehicleId, double distanceKm)
    {
        var vehicle = _context.Vehicles.Find(vehicleId);
        if (vehicle == null) return;

        vehicle.Mileage += distanceKm;
        _context.SaveChanges();

        Console.WriteLine($"Fahrzeug {vehicle.LicensePlate}: {distanceKm} km hinzugefügt. Neuer Kilometerstand: {vehicle.Mileage}");
    }

    public bool NeedsService(int vehicleId)
    {
        var v = _context.Vehicles.Find(vehicleId);
        if (v == null) return false;

        return v.Mileage >= 15000 && v.Mileage % 15000 <= 1000;
    }

    public List<Vehicle> GetAllByUser(string username)
    {
        var user = _context.Users.FirstOrDefault(u => u.Username == username);
        if (user == null) return new List<Vehicle>();

        return _context.Vehicles
            .Where(v => v.UserId == user.Id)
            .ToList();
    }
    
    public Vehicle? GetByIdAndUser(int id, string username)
    {
        var user = _context.Users.FirstOrDefault(u => u.Username == username);
        if (user == null) return null;

        return _context.Vehicles.FirstOrDefault(v => v.Id == id && v.UserId == user.Id);
    }

    public bool DeleteByUser(int vehicleId, string username)
    {
        var user = _context.Users.FirstOrDefault(u => u.Username == username);
        if (user == null) return false;

        var vehicle = _context.Vehicles.FirstOrDefault(v => v.Id == vehicleId && v.UserId == user.Id);
        if (vehicle == null) return false;

        _context.Vehicles.Remove(vehicle);
        _context.SaveChanges();
        return true;
    }

    public Vehicle AddForUser(Vehicle vehicle, string username, string role)
    {
        var currentUser = _context.Users.FirstOrDefault(u => u.Username == username);
        if (currentUser == null)
            throw new Exception("Benutzer nicht gefunden.");

        if (role == "Admin")
        {
            if (vehicle.UserId == 0)
            {
                vehicle.UserId = currentUser.Id;
            }
            else
            {
                var targetUser = _context.Users.FirstOrDefault(u => u.Id == vehicle.UserId);
                if (targetUser == null)
                    throw new Exception($"Benutzer mit ID {vehicle.UserId} existiert nicht.");
            }
        }
        else
        {
            vehicle.UserId = currentUser.Id;
        }

        _context.Vehicles.Add(vehicle);
        _context.SaveChanges();
        return vehicle;
    }

}
