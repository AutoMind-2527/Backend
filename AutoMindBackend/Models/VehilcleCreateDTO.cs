namespace AutoMindBackend.Models;

public class VehicleCreateDto
{
    public string LicensePlate { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public double Mileage { get; set; }
    public double FuelConsumption { get; set; }

    //nur für admin
    public int? UserId { get; set; }
}
