using AppPortal.Models;

namespace AppPortal.Services;

public class AppDraftEndpoint
{
    public string DisplayName { get; set; } = string.Empty;
    public string Scheme { get; set; } = "http";
    public int? HostPort { get; set; }
    public int? ContainerPort { get; set; }
    public bool Selected { get; set; } = true;
}

public class AppDraftModule
{
    public string Name { get; set; } = string.Empty;
    public string? SourceService { get; set; }
    public bool IsEnabled { get; set; } = true;
    public List<AppDraftEndpoint> Endpoints { get; set; } = new();
}

public class AppDraft
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ExternalId { get; set; }
    public List<AppDraftModule> Modules { get; set; } = new();
    public List<string> HostHints { get; set; } = new();
    public string SyncHint { get; set; } = string.Empty;
}

public sealed record ExternalCandidate(
    string ExternalId,
    string Name,
    string Kind,
    string Meta);

public interface IAppSource
{
    DataSourceType Type { get; }
    Task<bool> TestConnectionAsync(string configJson);
    Task<List<ExternalCandidate>> ListCandidatesAsync(string configJson);
    Task<AppDraft> ResolveAsync(string configJson, ExternalCandidate candidate);
}
