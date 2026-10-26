using AppPortal.Models;

namespace AppPortal.Services;

public class AppDraftAddress
{
    /// <summary>Full URL string (scheme required), same shape as AppModule.Addresses.</summary>
    public string Url { get; set; } = string.Empty;

    public bool Selected { get; set; } = true;

    /// <summary>Preview hint, e.g. 8080→80.</summary>
    public string? Label { get; set; }
}

public class AppDraftModule
{
    public string Name { get; set; } = string.Empty;
    public string? SourceService { get; set; }
    public bool IsEnabled { get; set; } = true;
    public List<AppDraftAddress> Addresses { get; set; } = new();
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

    /// <summary>列出候选；endpointId 标识 Portainer Environment（其他来源可忽略）。</summary>
    Task<List<ExternalCandidate>> ListCandidatesAsync(string configJson, int endpointId);

    Task<AppDraft> ResolveAsync(string configJson, int endpointId, ExternalCandidate candidate);
}
