using Microsoft.EntityFrameworkCore;
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

    // Admin: alle Fahrzeuge
    public List<Vehicle> GetAll()
    {
        return _context.Vehicles
            .Include(v => v.User)
            .Include(v => v.Trips)
            .ToList();
    }

    // User: alle Fahrzeuge des Users (über UserId)
    public List<Vehicle> GetAllByUserId(int userId)
    {
        return _context.Vehicles
            .Include(v => v.User)
            .Include(v => v.Trips)
            .Where(v => v.UserId == userId)
            .ToList();
    }

    // Fahrzeug nach Id (Admin)
    public Vehicle? GetById(int id)
    {
        return _context.Vehicles
            .Include(v => v.User)
            .Include(v => v.Trips)
            .FirstOrDefault(v => v.Id == id);
    }

    // Fahrzeug nach Id und UserId (User darf nur eigene sehen)
    public Vehicle? GetByIdAndUserId(int id, int userId)
    {
        return _context.Vehicles
            .Include(v => v.User)
            .Include(v => v.Trips)
            .FirstOrDefault(v => v.Id == id && v.UserId == userId);
    }

    // Prüfen ob Service fällig ist
    public bool NeedsService(int vehicleId)
    {
        var vehicle = _context.Vehicles
            .Include(v => v.Trips)
            .FirstOrDefault(v => v.Id == vehicleId);

        if (vehicle == null)
            return false;

        // Beispiel: Service alle 10.000 km
        var totalDistance = vehicle.Trips.Sum(t => t.DistanceKm);
        return (vehicle.Mileage + totalDistance) >= 10000;
    }

    // Service-Status mit Details
    public object GetServiceStatus(int vehicleId)
    {
        var vehicle = _context.Vehicles
            .Include(v => v.Trips)
            .FirstOrDefault(v => v.Id == vehicleId);

        if (vehicle == null)
            return new { needsService = false, message = "Fahrzeug nicht gefunden" };

        var totalDistance = vehicle.Trips.Sum(t => t.DistanceKm);
        var currentMileage = vehicle.Mileage + totalDistance;
        var serviceInterval = 10000;
        var kmUntilService = serviceInterval - (currentMileage % serviceInterval);
        var needsService = currentMileage >= serviceInterval;

        return new
        {
            needsService,
            currentMileage,
            kmUntilService,
            message = needsService ? "Service fällig" : $"Noch {kmUntilService} km bis zum Service"
        };
    }

    // Neues Fahrzeug anlegen
    public Vehicle Add(Vehicle vehicle)
    {
        _context.Vehicles.Add(vehicle);
        _context.SaveChanges();
        return vehicle;
    }

    // Fahrzeug updaten
    public Vehicle? Update(int id, VehicleUpdateDto dto)
    {
        var vehicle = _context.Vehicles.Find(id);
        if (vehicle == null)
            return null;

        vehicle.LicensePlate = dto.LicensePlate;
        vehicle.Brand = dto.Brand;
        vehicle.Model = dto.Model;
        vehicle.Mileage = dto.Mileage;
        vehicle.FuelConsumption = dto.FuelConsumption;

        _context.SaveChanges();
        return vehicle;
    }

    // Fahrzeug löschen (Admin)
    public bool Delete(int id)
    {
        var vehicle = _context.Vehicles.Find(id);
        if (vehicle == null)
            return false;

        _context.Vehicles.Remove(vehicle);
        _context.SaveChanges();
        return true;
    }

    // Optional: Fahrzeug vom Besitzer löschen (falls du das später brauchst)
    public bool DeleteByUserId(int id, int userId)
    {
        var vehicle = _context.Vehicles
            .FirstOrDefault(v => v.Id == id && v.UserId == userId);

        if (vehicle == null)
            return false;

        _context.Vehicles.Remove(vehicle);
        _context.SaveChanges();
        return true;
    }

    // Fahrzeuge, die Service brauchen (Admin)
    public List<Vehicle> GetVehiclesNeedingService()
    {
        var vehicles = _context.Vehicles
            .Include(v => v.User)
            .Include(v => v.Trips)
            .ToList();

        return vehicles.Where(v => NeedsService(v.Id)).ToList();
    }

    // Stats für ein Fahrzeug
    public object? GetVehicleStats(int vehicleId)
    {
        var vehicle = _context.Vehicles
            .Include(v => v.Trips)
            .FirstOrDefault(v => v.Id == vehicleId);

        if (vehicle == null)
            return null;

        var totalTrips = vehicle.Trips.Count;
        var totalDistance = vehicle.Trips.Sum(t => t.DistanceKm);
        var totalFuel = totalDistance * vehicle.FuelConsumption / 100;
        var avgTripDistance = totalTrips > 0 ? totalDistance / totalTrips : 0;

        return new
        {
            VehicleId = vehicleId,
            TotalTrips = totalTrips,
            TotalDistance = totalDistance,
            TotalFuelConsumed = totalFuel,
            AverageTripDistance = avgTripDistance,
            FuelConsumption = vehicle.FuelConsumption
        };
    }
}
