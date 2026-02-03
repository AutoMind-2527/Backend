using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
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
// Forwarded Headers (Ingress / Reverse Proxy)
// ----------------------------------------------------
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto |
        ForwardedHeaders.XForwardedHost;

    // In Kubernetes/Ingress kennt ASP.NET die Proxy-IP nicht -> erlauben
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// ----------------------------------------------------
// Keycloak Authentication (JWT Bearer)
// ----------------------------------------------------
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://if220129.cloud.htl-leonding.ac.at/keycloak/realms/automind-realm";
        options.RequireHttpsMetadata = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            NameClaimType = "preferred_username",
            RoleClaimType = ClaimTypes.Role
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                var identity = context.Principal?.Identity as ClaimsIdentity;
                if (identity == null) return Task.CompletedTask;

                // Rollen aus realm_access.roles
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
                                    identity.AddClaim(new Claim(ClaimTypes.Role, roleName));
                                }
                            }
                        }
                    }
                    catch (JsonException)
                    {
                        // ignorieren
                    }
                }

                // Username als Name setzen
                var preferredUsername = context.Principal.FindFirst("preferred_username")?.Value;
                if (!string.IsNullOrWhiteSpace(preferredUsername))
                {
                    identity.AddClaim(new Claim(ClaimTypes.Name, preferredUsername));
                }

                return Task.CompletedTask;
            }
        };
    });

// ----------------------------------------------------
// Swagger
// WICHTIG: Wir hängen Swagger explizit unter /api/swagger
// und setzen in der OpenAPI Spec den Server auf /api,
// damit "Try it out" auch /api/... aufruft.
// ----------------------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AutoMind API",
        Version = "v1"
    });

    // Security Definition (Bearer)
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
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
// Reverse Proxy headers MUSS vor allem anderen kommen
// ----------------------------------------------------
app.UseForwardedHeaders();

// ----------------------------------------------------
// Swagger immer aktiv (du hast ja true drin)
// ----------------------------------------------------
if (/*app.Environment.IsDevelopment()*/ true)
{
    // swagger.json unter /api/swagger/v1/swagger.json
    app.UseSwagger(c =>
    {
        c.RouteTemplate = "api/swagger/{documentName}/swagger.json";
        c.PreSerializeFilters.Add((swagger, httpReq) =>
        {
            // sorgt dafür, dass "Try it out" /api/... verwendet
            swagger.Servers = new List<OpenApiServer>
            {
                new OpenApiServer { Url = "/api" }
            };
        });
    });

    // Swagger UI unter /api/swagger
    app.UseSwaggerUI(c =>
    {
        c.RoutePrefix = "api/swagger";
        c.SwaggerEndpoint("/api/swagger/v1/swagger.json", "AutoMind API v1");

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
