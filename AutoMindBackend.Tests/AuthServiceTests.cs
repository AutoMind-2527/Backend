using AutoMindBackend.Data;
using AutoMindBackend.Models;
using AutoMindBackend.Services;
using FluentAssertions;
using Xunit;

namespace AutoMindBackend.Tests;

public class AuthServiceTests : TestBase
{
    [Fact]
    public void CreateLocalUser_ShouldCreateUserWithHashedPassword()
    {
        var ctx = CreateContext();
        var service = new AuthService(ctx);

        var user = service.CreateLocalUser("andrej", "secret123");

        user.Id.Should().BeGreaterThan(0);
        user.Username.Should().Be("andrej");
        user.Role.Should().Be("User");
        user.PasswordHash.Should().NotBeNullOrEmpty();
        BCrypt.Net.BCrypt.Verify("secret123", user.PasswordHash).Should().BeTrue();
    }

    [Fact]
    public void CreateLocalUser_ShouldThrow_WhenUsernameAlreadyExists()
    {
        var ctx = CreateContext();
        ctx.Users.Add(new User
        {
            Username = "andrej",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("pw"),
            Role = "User"
        });
        ctx.SaveChanges();

        var service = new AuthService(ctx);

        Action act = () => service.CreateLocalUser("andrej", "secret123");

        act.Should().Throw<Exception>()
            .WithMessage("*Benutzername bereits vergeben*");
    }

    [Fact]
    public void ValidateLocalUser_ShouldReturnUser_WhenPasswordIsCorrect()
    {
        var ctx = CreateContext();
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("secret123");

        ctx.Users.Add(new User
        {
            Username = "andrej",
            PasswordHash = passwordHash,
            Role = "User"
        });
        ctx.SaveChanges();

        var service = new AuthService(ctx);

        var user = service.ValidateLocalUser("andrej", "secret123");

        user.Should().NotBeNull();
        user.Username.Should().Be("andrej");
    }

    [Fact]
    public void ValidateLocalUser_ShouldThrow_WhenUserDoesNotExist()
    {
        var ctx = CreateContext();
        var service = new AuthService(ctx);

        Action act = () => service.ValidateLocalUser("unknown", "pw");

        act.Should().Throw<Exception>()
            .WithMessage("*Benutzer nicht gefunden*");
    }

    [Fact]
    public void ValidateLocalUser_ShouldThrow_WhenPasswordIsWrong()
    {
        var ctx = CreateContext();
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("correct");

        ctx.Users.Add(new User
        {
            Username = "andrej",
            PasswordHash = passwordHash,
            Role = "User"
        });
        ctx.SaveChanges();

        var service = new AuthService(ctx);

        Action act = () => service.ValidateLocalUser("andrej", "wrong");

        act.Should().Throw<Exception>()
            .WithMessage("*Ungültiges Passwort*");
    }

    [Fact]
    public void SyncKeycloakUser_ShouldCreateOrUpdateUser()
    {
        var ctx = CreateContext();
        var service = new AuthService(ctx);

        // 1. Neuer Keycloak User
        var created = service.SyncKeycloakUser(
            keycloakUserId: "kc-123",
            username: "andrej",
            email: "a@test.com",
            role: "User"
        );

        created.KeycloakId.Should().Be("kc-123");
        created.Username.Should().Be("andrej");
        created.Email.Should().Be("a@test.com");
        created.Role.Should().Be("User");

        // 2. Gleicher Keycloak User -> Update
        var updated = service.SyncKeycloakUser(
            keycloakUserId: "kc-123",
            username: "andrej_new",
            email: "new@test.com",
            role: "Admin"
        );

        updated.Id.Should().Be(created.Id);
        updated.Username.Should().Be("andrej_new");
        updated.Email.Should().Be("new@test.com");
        updated.Role.Should().Be("Admin");
    }
}
