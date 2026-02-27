using System.Security.Claims;

namespace AutoMindBackend.Services;

public class AuditUserProvider : IAuditUserProvider
{
    private readonly IHttpContextAccessor _http;

    public AuditUserProvider(IHttpContextAccessor http)
    {
        _http = http;
    }

    public string? GetUserId()
        => _http.HttpContext?.User?.FindFirstValue("sub");

    public string? GetUserName()
        => _http.HttpContext?.User?.Identity?.Name
           ?? _http.HttpContext?.User?.FindFirstValue("preferred_username");

    public string? GetIpAddress()
        => _http.HttpContext?.Connection?.RemoteIpAddress?.ToString();
}