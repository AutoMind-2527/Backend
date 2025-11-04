using AutoMindBackend.Models;
using AutoMindBackend.Services;
using FluentAssertions;
using Xunit;

namespace AutoMindBackend.Tests;

public class TripServiceTests : TestBase
{
    [Fact]
    public void Add_ShouldCalculateTripValues()
    {
        var ctx = CreateContext();
        ctx.Vehicles.Add(new Vehicle { Id = 1, FuelConsumption = 10 });
        ctx.SaveChanges();

        var service = new TripService(ctx);
        var trip = new Trip
        {
            VehicleId = 1,
            StartTime = DateTime.Now.AddHours(-2),
            EndTime = DateTime.Now,
            DistanceKm = 100
        };

        var result = service.Add(trip);

        result.AverageSpeed.Should().BeApproximately(50, 0.5);
        result.FuelUsed.Should().BeApproximately(10, 0.01);
        result.TripCost.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Add_ShouldThrow_WhenVehicleMissing()
    {
        var ctx = CreateContext();
        var service = new TripService(ctx);
        var trip = new Trip { VehicleId = 999, StartTime = DateTime.Now, EndTime = DateTime.Now.AddMinutes(10) };

        Action act = () => service.Add(trip);

        act.Should().Throw<Exception>().WithMessage("*Fahrzeug nicht gefunden*");
    }

    [Fact]
    public void GetTotalDistance_ShouldSumUpCorrectly()
    {
        var ctx = CreateContext();
        ctx.Trips.AddRange(
            new Trip { VehicleId = 1, DistanceKm = 50 },
            new Trip { VehicleId = 1, DistanceKm = 100 }
        );
        ctx.SaveChanges();

        var service = new TripService(ctx);
        service.GetTotalDistance(1).Should().Be(150);
    }

    [Fact]
    public void GetAverageConsumption_ShouldReturnCorrectValue()
    {
        var ctx = CreateContext();
        ctx.Trips.AddRange(
            new Trip { VehicleId = 1, DistanceKm = 100, FuelUsed = 5 },
            new Trip { VehicleId = 1, DistanceKm = 100, FuelUsed = 7 }
        );
        ctx.SaveChanges();

        var service = new TripService(ctx);
        service.GetAverageConsumption(1).Should().BeApproximately(6, 0.1);
    }
}
