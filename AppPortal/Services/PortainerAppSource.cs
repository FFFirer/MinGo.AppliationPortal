using AppPortal.Models;

namespace AppPortal.Services;

public class PortainerAppSource : IAppSource
{
    private readonly IPortainerClient _client;

    public PortainerAppSource(IPortainerClient client)
    {
        _client = client;
    }

    public DataSourceType Type => DataSourceType.Portainer;

    public Task<bool> TestConnectionAsync(string configJson)
        => _client.TestAsync(configJson);

    public async Task<List<ExternalCandidate>> ListCandidatesAsync(string configJson, int endpointId)
    {
        var projects = await _client.ListProjectsAsync(configJson, endpointId);
        return projects
            .Select(p => new ExternalCandidate(
                p.Name,
                p.Name,
                "compose",
                $"endpoint={p.EndpointId}, {p.Status}"))
            .ToList();
    }

    public async Task<AppDraft> ResolveAsync(string configJson, int endpointId, ExternalCandidate candidate)
    {
        // ExternalId 即 compose project name，保证列表 / 预览 / 回传一致。
        var projectName = candidate.ExternalId;
        var ports = await _client.GetProjectPortsAsync(configJson, endpointId, projectName);

        var draft = new AppDraft
        {
            Name = projectName,
            Description = $"Portainer compose · {projectName}",
            ExternalId = projectName
        };

        foreach (var group in ports.GroupBy(p => p.ServiceName ?? "default"))
        {
            var module = new AppDraftModule
            {
                Name = group.Key,
                SourceService = group.Key,
                IsEnabled = true
            };

            foreach (var mapping in group)
            {
                var pub = mapping.PublicPort;
                var label = pub is { } p
                    ? (mapping.PrivatePort > 0 && mapping.PrivatePort != p ? $"{p}→{mapping.PrivatePort}" : $"{p}")
                    : $"{mapping.PrivatePort}";

                // Placeholder URL; import UI rewrites host/scheme/path before confirm.
                var url = pub is { } port
                    ? $"http://127.0.0.1:{port}/"
                    : $"http://127.0.0.1/";

                module.Addresses.Add(new AppDraftAddress
                {
                    Url = url,
                    Label = label,
                    Selected = pub is not null
                });
            }

            if (module.Addresses.Count > 0)
            {
                draft.Modules.Add(module);
            }
        }

        draft.SyncHint = ComputeSyncHint(draft);
        return draft;
    }

    private static string ComputeSyncHint(AppDraft draft)
    {
        var parts = draft.Modules
            .OrderBy(m => m.Name)
            .Select(m =>
            {
                var eps = string.Join(",", m.Addresses
                    .OrderBy(a => a.Label)
                    .Select(a => a.Label ?? a.Url));
                return $"{m.Name}|{eps}";
            });
        return string.Join(";", parts);
    }
}
