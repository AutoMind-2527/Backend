using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using AutoMindBackend.Data;
using AutoMindBackend.Services;

var builder = WebApplication.CreateBuilder(args);

// ----------------------------------------------------
// JSON Serializer Config
// ----------------------------------------------------
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });

// ----------------------------------------------------
// Database
// ----------------------------------------------------
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=automind.db"));

// ----------------------------------------------------
// Keycloak Authentication (JWT Bearer)
// ----------------------------------------------------
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "http://localhost:8080/realms/automind-realm";
        options.RequireHttpsMetadata = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            // Audience-Check erstmal aus, damit der "empty audience"-Fehler weg ist
            ValidateAudience = false,
            // Username aus Keycloak
            NameClaimType = "preferred_username",
            // Wir erzeugen eigene Role-Claims
            RoleClaimType = ClaimTypes.Role
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                var identity = context.Principal?.Identity as ClaimsIdentity;
                if (identity == null)
                    return Task.CompletedTask;

                // ---------- Rollen aus realm_access.roles ----------
                var realmAccessClaim = context.Principal.FindFirst("realm_access");
                if (realmAccessClaim?.Value is string realmAccessJson)
                {
                    try
                    {
                        using var doc = JsonDocument.Parse(realmAccessJson);
                        if (doc.RootElement.TryGetProperty("roles", out var rolesElement) &&
                            rolesElement.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var roleElement in rolesElement.EnumerateArray())
                            {
                                var roleName = roleElement.GetString();
                                if (!string.IsNullOrWhiteSpace(roleName))
                                {
                                    // z.B. "Admin" oder "User"
                                    identity.AddClaim(new Claim(ClaimTypes.Role, roleName));
                                }
                            }
                        }
                    }
                    catch (JsonException)
                    {
                        // falls Keycloak hier mal etwas Unerwartetes liefert -> ignorieren
                    }
                }

                // ---------- Username als Name setzen ----------
                var preferredUsername =
                    context.Principal.FindFirst("preferred_username")?.Value;

                if (!string.IsNullOrWhiteSpace(preferredUsername))
                {
                    identity.AddClaim(new Claim(ClaimTypes.Name, preferredUsername));
                }

                return Task.CompletedTask;
            }
        };
    });

// ----------------------------------------------------
// Swagger – OAuth2 (Password Flow) gegen Keycloak
// ----------------------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title   = "AutoMind API",
        Version = "v1"
    });

    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Type  = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            Password = new OpenApiOAuthFlow
            {
                TokenUrl = new Uri("http://localhost:8080/realms/automind-realm/protocol/openid-connect/token"),
                Scopes = new Dictionary<string, string>
                {
                    { "openid",  "OpenID Connect" },
                    { "profile", "User Profile" },
                    { "email",   "User Email" }
                }
            }
        }
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "oauth2"
                }
            },
            new[] { "openid", "profile", "email" }
        }
    });
});

// ----------------------------------------------------
// Services
// ----------------------------------------------------
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<VehicleService>();
builder.Services.AddScoped<TripService>();
builder.Services.AddScoped<GpsService>();
builder.Services.AddScoped<UserSyncService>();
builder.Services.AddScoped<DataSeeder>();

// ----------------------------------------------------
// CORS
// ----------------------------------------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod()
    );
});

var app = builder.Build();

// ----------------------------------------------------
// Development Mode
// ----------------------------------------------------
if (/*app.Environment.IsDevelopment()*/ true)
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("v1/swagger.json", "AutoMind API");

        // Keycloak OAuth2 Settings für Swagger-Login
        c.OAuthClientId("automind-backend");
        c.OAuthClientSecret(builder.Configuration["Keycloak:ClientSecret"]);
        c.OAuthAppName("AutoMind Backend");

        // Authorization bleibt nach Refresh erhalten
        c.ConfigObject.AdditionalItems["persistAuthorization"] = true;
    });
}

// ----------------------------------------------------
// DB Init (EnsureCreated + Seed)
// ----------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
    seeder.SeedData();
}

// ----------------------------------------------------
// Middleware
// ----------------------------------------------------
app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();