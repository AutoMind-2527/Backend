namespace AutoMindBackend.Models;

public class Trip
{
    public int Id { get; set; }

    public DateTime StartTime { get; set; }

    // GANZ WICHTIG: nullable !
    public DateTime? EndTime { get; set; }

    public double DistanceKm { get; set; }
    public string StartLocation { get; set; } = string.Empty;
    public string EndLocation { get; set; } = string.Empty;

    public double MaxSpeedKmh { get; set; }
    public double FuelUsedLiters { get; set; }
    public double FuelCost { get; set; }

    public int VehicleId { get; set; }
    public int UserId { get; set; }

    public Vehicle Vehicle { get; set; } = null!;
    public User User { get; set; } = null!;

    public List<GpsData> GpsPoints { get; set; } = new();
}
