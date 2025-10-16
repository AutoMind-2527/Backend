namespace AutoMindBackend.Models;

public class GpsData
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
}
