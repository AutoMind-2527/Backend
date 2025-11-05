using AutoMindBackend.Data;
using AutoMindBackend.Services;
using AutoMindBackend.Models;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace AutoMindBackend.Tests;

public class AuthServiceTests : TestBase
{
    private IConfiguration CreateConfig() =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Jwt:Key", "SuperSecretKey1234567890" }
            })
            .Build();

    [Fact]
    public void Register_ShouldCreateNewUser()
    {
        var ctx = CreateContext();
        var auth = new AuthService(ctx, CreateConfig());

        var user = auth.Register("andrej", "password123");

        user.Username.Should().Be("andrej");
        user.PasswordHash.Should().NotBeEmpty();
        user.PasswordSalt.Should().NotBeEmpty();
        user.Role.Should().Be("User");
    }

    [Fact]
    public void Register_ShouldThrow_WhenDuplicateUser()
    {
        var ctx = CreateContext();
        ctx.Users.Add(new User { Username = "andrej", PasswordHash = [1], PasswordSalt = [1] });
        ctx.SaveChanges();

        var auth = new AuthService(ctx, CreateConfig());

        Action act = () => auth.Register("andrej", "pw");
        act.Should().Throw<Exception>().WithMessage("*existiert bereits*");
    }

    [Fact]
    public void Login_ShouldThrow_WhenWrongPassword()
    {
        var ctx = CreateContext();
        var auth = new AuthService(ctx, CreateConfig());
        auth.Register("andrej", "right");

        Action act = () => auth.Login("andrej", "wrong");
        act.Should().Throw<Exception>().WithMessage("*Falsches Passwort*");
    }
}
