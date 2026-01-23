using AutoMindBackend.Data;
using AutoMindBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoMindBackend.Services;

public class GpsService
{
    private readonly AppDbContext _context;

    // Config (später evtl. aus appsettings holen)
    private const double InactivitySeconds = 30;             // > 30s Pause => Trip Ende
    private const double DefaultConsumptionLPer100Km = 7.0;  // z.B. 7 l/100km
    private const double DefaultFuelPricePerLiter = 1.6;     // €/l

    public GpsService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gerät schickt alle ~30 Sekunden einen GPS-Punkt.
    /// - Punkt speichern
    /// - offenen Trip finden/erstellen
    /// - falls letzte Position zu alt -> Trip beenden + auswerten
    /// </summary>
    public async Task AddGpsPointAsync(int vehicleId, double lat, double lon, double? speedKmh = null)
    {
        var now = DateTime.UtcNow;

        // letztes GPS dieses Fahrzeugs
        var lastPoint = await _context.Set<GpsData>()
            .Where(g => g.VehicleId == vehicleId)
            .OrderByDescending(g => g.Timestamp)
            .FirstOrDefaultAsync();

        // offenen Trip suchen
        var openTrip = await _context.Trips
            .Where(t => t.VehicleId == vehicleId && t.EndTime == null)
            .Include(t => t.GpsPoints)
            .FirstOrDefaultAsync();

        // wenn Trip offen & letzte Position zu alt -> Trip schließen
        if (openTrip != null && lastPoint != null)
        {
            var diffSeconds = (now - lastPoint.Timestamp).TotalSeconds;
            if (diffSeconds > InactivitySeconds)
            {
                await CloseTripAsync(openTrip);
                openTrip = null;
            }
        }

        // wenn kein Trip offen -> neuen Trip eröffnen
        if (openTrip == null)
        {
            var vehicle = await _context.Vehicles.FirstAsync(v => v.Id == vehicleId);

            openTrip = new Trip
            {
                VehicleId = vehicleId,
                UserId = vehicle.UserId,
                StartTime = now,
                EndTime = null,
                StartLocation = $"{lat},{lon}",
                EndLocation = $"{lat},{lon}"
            };


            _context.Trips.Add(openTrip);
            await _context.SaveChangesAsync();
        }


        // ✅ IMMER den letzten Punkt als EndLocation setzen
        openTrip.EndLocation = $"{lat},{lon}";
        await _context.SaveChangesAsync();


        // neuen GPS-Punkt speichern
        var gps = new GpsData
        {
            VehicleId = vehicleId,
            Latitude = lat,
            Longitude = lon,
            SpeedKmh = speedKmh,
            Timestamp = now,
            TripId = openTrip.Id
        };

        _context.Set<GpsData>().Add(gps);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Alle GPS-Daten (z.B. für Admin-Debug).
    /// </summary>
    public async Task<List<GpsData>> GetAllGpsDataAsync()
    {
        return await _context.Set<GpsData>()
            .OrderByDescending(g => g.Timestamp)
            .ToListAsync();
    }

    /// <summary>
    /// Reiner Simulations-Endpoint, speichert nichts in der DB,
    /// berechnet nur eine einfache Distanz "Trip-Preview".
    /// </summary>
    public object CreateTripFromGps(double startLat, double startLon, double endLat, double endLon, int vehicleId)
    {
        var distanceKm = HaversineKm(startLat, startLon, endLat, endLon);

        var fuelUsed = distanceKm * DefaultConsumptionLPer100Km / 100.0;
        var fuelCost = fuelUsed * DefaultFuelPricePerLiter;

        return new
        {
            VehicleId = vehicleId,
            StartLat = startLat,
            StartLon = startLon,
            EndLat = endLat,
            EndLon = endLon,
            DistanceKm = distanceKm,
            FuelUsedLiters = fuelUsed,
            FuelCost = fuelCost
        };
    }

    /// <summary>
    /// Trip beenden und alle Kennzahlen berechnen.
    /// </summary>
    private async Task CloseTripAsync(Trip trip)
    {
        var points = await _context.Set<GpsData>()
            .Where(g => g.TripId == trip.Id)
            .OrderBy(g => g.Timestamp)
            .ToListAsync();

        if (points.Count == 0)
        {
            trip.EndTime = DateTime.UtcNow;
            trip.DistanceKm = 0;
            trip.MaxSpeedKmh = 0;
            trip.FuelUsedLiters = 0;
            trip.FuelCost = 0;
            await _context.SaveChangesAsync();
            return;
        }

        if (points.Count == 1)
        {
            var p = points[0];
            trip.StartTime = p.Timestamp;
            trip.EndTime = p.Timestamp;
            trip.DistanceKm = 0;
            trip.MaxSpeedKmh = p.SpeedKmh ?? 0;
            trip.FuelUsedLiters = 0;
            trip.FuelCost = 0;
            trip.StartLocation = $"{p.Latitude},{p.Longitude}";
            trip.EndLocation = trip.StartLocation;
            await _context.SaveChangesAsync();
            return;
        }

        trip.StartTime = points.First().Timestamp;
        trip.EndTime = points.Last().Timestamp;

        trip.DistanceKm = CalculateTotalDistance(points);
        trip.MaxSpeedKmh = CalculateMaxSpeed(points);

        trip.StartLocation = $"{points.First().Latitude},{points.First().Longitude}";
        trip.EndLocation = $"{points.Last().Latitude},{points.Last().Longitude}";

        trip.FuelUsedLiters = trip.DistanceKm * DefaultConsumptionLPer100Km / 100.0;
        trip.FuelCost = trip.FuelUsedLiters * DefaultFuelPricePerLiter;

        await _context.SaveChangesAsync();
    }

    private double CalculateTotalDistance(List<GpsData> points)
    {
        double total = 0;

        for (int i = 1; i < points.Count; i++)
        {
            total += HaversineKm(
                points[i - 1].Latitude, points[i - 1].Longitude,
                points[i].Latitude, points[i].Longitude);
        }

        return total;
    }

    private double CalculateMaxSpeed(List<GpsData> points)
    {
        double maxSpeed = 0;

        for (int i = 1; i < points.Count; i++)
        {
            var p1 = points[i - 1];
            var p2 = points[i];

            var distanceKm = HaversineKm(p1.Latitude, p1.Longitude, p2.Latitude, p2.Longitude);
            var seconds = (p2.Timestamp - p1.Timestamp).TotalSeconds;

            if (seconds > 0)
            {
                var speedKmh = distanceKm / (seconds / 3600.0);
                if (speedKmh > maxSpeed)
                    maxSpeed = speedKmh;
            }

            if (p2.SpeedKmh.HasValue && p2.SpeedKmh.Value > maxSpeed)
                maxSpeed = p2.SpeedKmh.Value;
        }

        return maxSpeed;
    }

    private double HaversineKm(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371.0;

        double dLat = ToRadians(lat2 - lat1);
        double dLon = ToRadians(lon2 - lon1);

        double a =
            Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
            Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
            Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    private double ToRadians(double deg) => deg * Math.PI / 180.0;
}
