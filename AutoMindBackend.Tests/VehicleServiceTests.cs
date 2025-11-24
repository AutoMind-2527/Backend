using AutoMindBackend.Models;
using AutoMindBackend.Services;
using FluentAssertions;
using Xunit;

namespace AutoMindBackend.Tests;

public class VehicleServiceTests : TestBase
{
    [Fact]
    public void Add_ShouldSaveVehicle()
    {
        var ctx = CreateContext();
        var service = new VehicleService(ctx);

        var vehicle = new Vehicle
        {
            LicensePlate = "L-123AA",
            Brand = "BMW",
            Model = "i4",
            Mileage = 1000,
            FuelConsumption = 6.5,
            UserId = 1
        };

        var result = service.Add(vehicle);

        result.Id.Should().BeGreaterThan(0);
        ctx.Vehicles.Count().Should().Be(1);
        ctx.Vehicles.Single().LicensePlate.Should().Be("L-123AA");
    }

    [Fact]
    public void NeedsService_ShouldReturnTrue_WhenMileagePlusTripsOver10000()
    {
        var ctx = CreateContext();

        var vehicle = new Vehicle
        {
            Id = 1,
            LicensePlate = "L-111AA",
            Brand = "BMW",
            Model = "i4",
            Mileage = 9500,
            FuelConsumption = 6.5,
            UserId = 1,
            Trips = new List<Trip>
            {
                new Trip
                {
                    Id = 1,
                    VehicleId = 1,
                    UserId = 1,
                    DistanceKm = 600,
                    StartTime = DateTime.Now.AddHours(-2),
                    EndTime = DateTime.Now.AddHours(-1),
                    StartLocation = "A",
                    EndLocation = "B"
                }
            }
        };

        ctx.Vehicles.Add(vehicle);
        ctx.SaveChanges();

        var service = new VehicleService(ctx);

        var result = service.NeedsService(1);

        result.Should().BeTrue();
    }
}
