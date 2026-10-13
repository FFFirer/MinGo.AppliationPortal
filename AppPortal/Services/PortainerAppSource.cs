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
            ?? stacks.FirstOrDefault(s => string.Equals(s.Name, candidate.ExternalId, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"未找到 stack {candidate.ExternalId}");

        var ports = await _client.GetStackPortsAsync(configJson, stack);
        var draft = new AppDraft
        {
            Name = stack.Name,
            Description = $"Portainer stack · {stack.Name}",
            // Must match ListCandidatesAsync ExternalId (stack Id), or form post cannot resolve.
            ExternalId = stack.Id.ToString()
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
