namespace AutoMindBackend.Models;

public class Trip
{
    public int Id { get; set; }

    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }  // nullable = Trip kann noch laufen

    public double DistanceKm { get; set; }
    public string StartLocation { get; set; } = string.Empty;
    public string EndLocation { get; set; } = string.Empty;

    // Aus GPS-Daten berechnet:
    public double MaxSpeedKmh { get; set; }
    public double FuelUsedLiters { get; set; }
    public double FuelCost { get; set; }

    // Foreign Keys
    public int VehicleId { get; set; }
    public int UserId { get; set; }

    // Navigation Properties
    public Vehicle Vehicle { get; set; } = null!;
    public User User { get; set; } = null!;

    public List<GpsData> GpsPoints { get; set; } = new();
}
