using AutoMindBackend.Data;
using AutoMindBackend.Models;

namespace AutoMindBackend.Services;

public class GpsService
{
    private readonly AppDbContext _context;

    public GpsService(AppDbContext context)
    {
        _context = context;
    }

    // Temporär: Erstelle einfache Implementierung
    public object CreateTripFromGps(double startLat, double startLon, double endLat, double endLon, int vehicleId)
    {
        // Beispiel-Implementierung
        var trip = new
        {
            StartLat = startLat,
            StartLon = startLon,
            EndLat = endLat,
            EndLon = endLon,
            VehicleId = vehicleId,
            Distance = CalculateDistance(startLat, startLon, endLat, endLon),
            Message = "GPS-Trip simuliert"
        };
        
        return trip;
    }

    private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        // Einfache Distanzberechnung
        return Math.Sqrt(Math.Pow(lat2 - lat1, 2) + Math.Pow(lon2 - lon1, 2)) * 111; // km
    }

    public List<object> GetAllGpsData()
    {
        // Temporär: Leere Liste zurückgeben
        return new List<object>();
    }
}