using AutoMindBackend.Data;
using AutoMindBackend.Models;

namespace AutoMindBackend.Services;

public class DataSeeder
{
    private readonly AppDbContext _context;

    public DataSeeder(AppDbContext context)
    {
        _context = context;
    }

    public void SeedData()
    {
        // Wenn schon User existieren -> nichts machen
        if (_context.Users.Any())
            return;

        // Admin-User anlegen (für Keycloak-Sync ist der KeycloakId-Wert egal,
        // wichtig ist nur, dass ein Admin existiert, falls du ihn brauchst)
        var admin = new User
        {
            KeycloakId = "admin-keycloak-id",
            Username = "admin",
            Email = "admin@automind.com",
            Role = "Admin",
            PasswordHash = "",
            PasswordSalt = ""
        };

        _context.Users.Add(admin);
        _context.SaveChanges();

        // KEINE Vehicles / Trips hier seeden -> sonst FK-Probleme,
        // weil wir keine gültigen UserIds / Keycloak-User haben.
    }
}
