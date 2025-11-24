using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using AutoMindBackend.Data;
using AutoMindBackend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// --- CONTROLLERS + JSON (Zyklen ignorieren) ---
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        // Funktioniert in allen .NET Versionen:
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = false;
    });

// --- DATABASE ---
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=automind.db"));

// --- KEYCLOAK AUTHENTICATION ---
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MetadataAddress =
            $"{builder.Configuration["Keycloak:Authority"]}/.well-known/openid-configuration";

        options.RequireHttpsMetadata = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            RoleClaimType = "roles"
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                var identity = context.Principal!.Identity as ClaimsIdentity;
                if (identity == null)
                    return Task.CompletedTask;

                var preferredUsername = identity.FindFirst("preferred_username")?.Value;
                if (!string.IsNullOrWhiteSpace(preferredUsername))
                {
                    identity.AddClaim(new Claim(ClaimTypes.Name, preferredUsername));
                }

                var realmAccess = identity.FindFirst("realm_access")?.Value;
                if (!string.IsNullOrWhiteSpace(realmAccess))
                {
                    try
                    {
                        using var doc = JsonDocument.Parse(realmAccess);
                        if (doc.RootElement.TryGetProperty("roles", out var rolesElement))
                        {
                            foreach (var role in rolesElement.EnumerateArray())
                            {
                                var r = role.GetString();
                                if (!string.IsNullOrWhiteSpace(r))
                                    identity.AddClaim(new Claim("roles", r));
                            }
                        }
                    }
                    catch
                    {
                    }
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// --- SWAGGER / OPENAPI ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AutoMind API",
        Version = "v1",
        Description = "AutoMind Backend API with Keycloak Authentication"
    });

    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            Password = new OpenApiOAuthFlow
            {
                TokenUrl = new Uri(
                    $"{builder.Configuration["Keycloak:Authority"]}/protocol/openid-connect/token"),
                Scopes = new Dictionary<string, string>
                {
                    { "openid", "OpenID Connect" },
                    { "profile", "User Profile" },
                    { "email", "Email Address" }
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
                    Id = "oauth2"
                }
            },
            new[] { "openid", "profile", "email" }
        }
    });
});

// --- SERVICES ---
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UserSyncService>();
builder.Services.AddScoped<TripService>();
builder.Services.AddScoped<VehicleService>();
builder.Services.AddScoped<GpsService>();

// --- CORS ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();

    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "AutoMind API v1");

        c.OAuthClientId("automind-backend");
        c.OAuthClientSecret(builder.Configuration["Keycloak:ClientSecret"]);
        c.OAuthUsePkce();
        c.OAuthAppName("AutoMind API");

        c.ConfigObject.AdditionalItems["persistAuthorization"] = true;
    });
}

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();
}

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
