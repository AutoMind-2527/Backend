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

    public List<GpsData> GetAllGpsData()
    {
        return _context.GpsData.ToList();
    }

    public void SaveGpsData(GpsData gpsData)
    {
        _context.Add(gpsData);
        _context.SaveChanges();
    }

    public Trip CreateTripFromGps(double startLat, double startLon, double endLat, double endLon, int vehicleId)
    {
        // Beispielhafte Berechnung, du kannst sie anpassen
        double distanceKm = 120; 
        DateTime startTime = DateTime.Now.AddHours(-2);
        DateTime endTime = DateTime.Now;

        // 🔹 Trip erzeugen
        var trip = new Trip
        {
            StartTime = startTime,
            EndTime = endTime,
            DistanceKm = distanceKm,
            StartLocation = $"({startLat}, {startLon})",
            EndLocation = $"({endLat}, {endLon})",
            VehicleId = vehicleId
        };

        _tripService.Add(trip);
        _vehicleService.AddMileage(vehicleId, distanceKm);

        // 🔹 GPS-Daten hinzufügen
        var gpsStart = new GpsData
        {
            VehicleId = vehicleId,
            Latitude = startLat,
            Longitude = startLon,
            Timestamp = startTime
        };
        var gpsEnd = new GpsData
        {
            VehicleId = vehicleId,
            Latitude = endLat,
            Longitude = endLon,
            Timestamp = endTime
        };

        _context.GpsData.AddRange(gpsStart, gpsEnd);
        _context.SaveChanges();

        Console.WriteLine($"✅ Neuer Trip + GPS-Daten für Fahrzeug {vehicleId} gespeichert.");
        return trip;
    }
}
