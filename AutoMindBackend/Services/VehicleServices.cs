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

    // Get all vehicles (Admin only)
    public List<Vehicle> GetAll()
    {
        return _context.Vehicles
            .Include(v => v.User)
            .Include(v => v.Trips)
            .ToList();
    }

    // Get vehicles by Keycloak user
    public List<Vehicle> GetAllByKeycloakUser(string keycloakUserId)
    {
        return _context.Vehicles
            .Include(v => v.User)
            .Include(v => v.Trips)
            .Where(v => v.User.KeycloakId == keycloakUserId)
            .ToList();
    }

    // Get vehicle by ID
    public Vehicle? GetById(int id) // ✅ Nullable return type
    {
        return _context.Vehicles
            .Include(v => v.User)
            .Include(v => v.Trips)
            .FirstOrDefault(v => v.Id == id);
    }

    // Get vehicle by ID and Keycloak user
    public Vehicle? GetByIdAndKeycloakUser(int id, string keycloakUserId) // ✅ Nullable return type
    {
        return _context.Vehicles
            .Include(v => v.User)
            .Include(v => v.Trips)
            .FirstOrDefault(v => v.Id == id && v.User.KeycloakId == keycloakUserId);
    }

    // Check if vehicle needs service
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

    // Get service status with details
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

    // Create new vehicle
    public Vehicle Add(Vehicle vehicle)
    {
        _context.Vehicles.Add(vehicle);
        _context.SaveChanges();
        return vehicle;
    }

    // Update vehicle
    public Vehicle? Update(int id, VehicleUpdateDto dto) // ✅ Nullable return type
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

    // Delete vehicle
    public bool Delete(int id)
    {
        var vehicle = _context.Vehicles.Find(id);
        if (vehicle == null)
            return false;

        _context.Vehicles.Remove(vehicle);
        _context.SaveChanges();
        return true;
    }

    // Delete vehicle by user
    public bool DeleteByKeycloakUser(int id, string keycloakUserId)
    {
        var vehicle = _context.Vehicles
            .Include(v => v.User)
            .FirstOrDefault(v => v.Id == id && v.User.KeycloakId == keycloakUserId);

        if (vehicle == null)
            return false;

        _context.Vehicles.Remove(vehicle);
        _context.SaveChanges();
        return true;
    }

    // Get vehicles that need service
    public List<Vehicle> GetVehiclesNeedingService()
    {
        var vehicles = _context.Vehicles
            .Include(v => v.User)
            .Include(v => v.Trips)
            .ToList();

        return vehicles.Where(v => NeedsService(v.Id)).ToList();
    }

    // Get vehicle statistics
    public object? GetVehicleStats(int vehicleId) // ✅ Nullable return type
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