namespace AutoMindBackend.Models;

public class GpsData
{
    public int Id { get; set; }

    public int VehicleId { get; set; }

    public double Latitude { get; set; }
    public double Longitude { get; set; }

    /// <summary>
    /// Geschwindigkeit, falls das Gerät sie mitschickt (km/h).
    /// </summary>
    public double? SpeedKmh { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public int? TripId { get; set; }
    public Trip? Trip { get; set; }
}
