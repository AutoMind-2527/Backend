using AutoMindBackend.Models;
using AutoMindBackend.Services;
using FluentAssertions;
using Xunit;

namespace AutoMindBackend.Tests;

public class TripServiceTests : TestBase
{
    [Fact]
    public void Add_ShouldSaveTrip()
    {
        var ctx = CreateContext();
        var service = new TripService(ctx);

        var trip = new Trip
        {
            StartTime = DateTime.Now.AddHours(-1),
            EndTime = DateTime.Now,
            DistanceKm = 42,
            StartLocation = "Linz",
            EndLocation = "Wels",
            VehicleId = 1,
            UserId = 1
        };

        var result = service.Add(trip);

        result.Id.Should().BeGreaterThan(0);
        ctx.Trips.Count().Should().Be(1);
        ctx.Trips.Single().DistanceKm.Should().Be(42);
    }

    [Fact]
    public void Delete_ShouldRemoveTripAndReturnTrue_WhenTripExists()
    {
        var ctx = CreateContext();
        var trip = new Trip
        {
            Id = 10,
            StartTime = DateTime.Now.AddHours(-1),
            EndTime = DateTime.Now,
            DistanceKm = 5,
            StartLocation = "A",
            EndLocation = "B",
            VehicleId = 1,
            UserId = 1
        };
        ctx.Trips.Add(trip);
        ctx.SaveChanges();

        var service = new TripService(ctx);

        var result = service.Delete(10);

        result.Should().BeTrue();
        ctx.Trips.Should().BeEmpty();
    }
}
