namespace AutoMindBackend.Services;

public interface IAuditUserProvider
{
    string? GetUserId();
    string? GetUserName();
    string? GetIpAddress();
}