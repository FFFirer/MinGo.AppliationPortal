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

    public async Task<List<ExternalCandidate>> ListCandidatesAsync(string configJson)
    {
        var stacks = await _client.ListStacksAsync(configJson);
        return stacks
            .Select(s => new ExternalCandidate(
                s.Id.ToString(),
                s.Name,
                "stack",
                $"status={s.Status}, endpoint={s.EndpointId}"))
            .ToList();
    }

    public async Task<AppDraft> ResolveAsync(string configJson, ExternalCandidate candidate)
    {
        var stacks = await _client.ListStacksAsync(configJson);
        var stack = stacks.FirstOrDefault(s => s.Id.ToString() == candidate.ExternalId)
            ?? throw new InvalidOperationException($"未找到 stack {candidate.ExternalId}");

        var ports = await _client.GetStackPortsAsync(configJson, stack);
        var draft = new AppDraft
        {
            Name = stack.Name,
            Description = $"Portainer stack · {stack.Name}",
            ExternalId = stack.Name
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
                module.Endpoints.Add(new AppDraftEndpoint
                {
                    DisplayName = mapping.PublicPort is { } pub
                        ? $"{group.Key} · {pub}→{mapping.PrivatePort}"
                        : $"{group.Key} · {mapping.PrivatePort}",
                    Scheme = "http",
                    Address = draft.HostHints.FirstOrDefault() ?? "",
                    HostPort = mapping.PublicPort,
                    ContainerPort = mapping.PrivatePort,
                    Selected = mapping.PublicPort is not null
                });
            }

            draft.Modules.Add(module);
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
                var eps = string.Join(",",
                    m.Endpoints
                        .OrderBy(e => e.HostPort ?? 0)
                        .ThenBy(e => e.ContainerPort ?? 0)
                        .Select(e => $"{e.HostPort}:{e.ContainerPort}"));
                return $"{m.Name}|{eps}";
            });
        return string.Join(";", parts);
    }
}
