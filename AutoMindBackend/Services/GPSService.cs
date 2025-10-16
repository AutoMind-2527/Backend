using AutoMindBackend.Data;
using AutoMindBackend.Models;

namespace AutoMindBackend.Services;

public class GpsService
{
    private readonly AppDbContext _context;
    private readonly TripService _tripService;
    private readonly VehicleService _vehicleService;

    public GpsService(AppDbContext context, TripService tripService, VehicleService vehicleService)
    {
        _context = context;
        _tripService = tripService;
        _vehicleService = vehicleService;
    }

    public List<Vehicle> GetAll()
    {
        return _context.Vehicles.ToList();
    }

    // GPS-Position speichern
    public void SaveGpsData(GpsData gpsData)
    {
        _context.Add(gpsData);
        _context.SaveChanges();
    }

    // Simulierte Berechnung eines Trips aus GPS-Daten
    public Trip CreateTripFromGps(double startLat, double startLon, double endLat, double endLon, int vehicleId)
    {
        // 🔹 (In echt: Distanz mit Haversine-Formel berechnen)
        double simulatedDistance = 85.4; // km
        DateTime start = DateTime.Now.AddHours(-1.5);
        DateTime end = DateTime.Now;

        var trip = new Trip
        {
            StartTime = start,
            EndTime = end,
            DistanceKm = simulatedDistance,
            StartLocation = $"({startLat},{startLon})",
            EndLocation = $"({endLat},{endLon})",
            VehicleId = vehicleId
        };

        _tripService.Add(trip);
        _vehicleService.AddMileage(vehicleId, simulatedDistance);

        Console.WriteLine($"GPS-Trip erzeugt: {simulatedDistance} km für Fahrzeug {vehicleId}");

        return trip;
    }
}
