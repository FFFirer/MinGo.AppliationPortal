using AppPortal.Models;
using LiteDB;

namespace AppPortal.Services;

public interface IImportService
{
    IAppSource? ResolveSource(DataSourceType type);
    Task<List<ExternalCandidate>> ListCandidatesAsync(DataSource source, int endpointId);
    Task<AppDraft> ResolveDraftAsync(DataSource source, int endpointId, ExternalCandidate candidate);
    Task<AppEntry> ImportDraftAsync(DataSource source, ObjectId environmentId, AppDraft draft);
}

public class ImportService : IImportService
{
    private readonly IAppService _apps;
    private readonly IEnumerable<IAppSource> _sources;

    public ImportService(IAppService apps, IEnumerable<IAppSource> sources)
    {
        _apps = apps;
        _sources = sources;
    }

    public IAppSource? ResolveSource(DataSourceType type)
        => _sources.FirstOrDefault(s => s.Type == type);

    public async Task<List<ExternalCandidate>> ListCandidatesAsync(DataSource source, int endpointId)
    {
        var impl = ResolveSource(source.Type)
            ?? throw new InvalidOperationException($"未实现的数据源类型 {source.Type}");
        return await impl.ListCandidatesAsync(source.Config, endpointId);
    }

    public async Task<AppDraft> ResolveDraftAsync(DataSource source, int endpointId, ExternalCandidate candidate)
    {
        var impl = ResolveSource(source.Type)
            ?? throw new InvalidOperationException($"未实现的数据源类型 {source.Type}");
        return await impl.ResolveAsync(source.Config, endpointId, candidate);
    }

    public async Task<AppEntry> ImportDraftAsync(DataSource source, ObjectId environmentId, AppDraft draft)
    {
        var existing = (await _apps.GetAllAppsAsync())
            .FirstOrDefault(a => a.SourceId == source.Id
                              && a.EnvironmentId == environmentId
                              && a.ExternalId == draft.ExternalId);

        var modules = draft.Modules
            .Select((m, index) => new AppModule
            {
                Name = m.Name,
                SourceService = m.SourceService,
                IsEnabled = m.IsEnabled,
                Order = index,
                Addresses = m.Addresses
                    .Where(a => a.Selected)
                    .Select(a => (a.Url ?? "").Trim())
                    .Where(u => u.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                             || u.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    .ToList()
            })
            .ToList();

        if (existing is null)
        {
            var app = new AppEntry
            {
                Name = draft.Name,
                Description = draft.Description,
                Modules = modules,
                SourceId = source.Id,
                EnvironmentId = environmentId,
                ExternalId = draft.ExternalId,
                SyncHint = draft.SyncHint,
                IsEnabled = true
            };
            await _apps.CreateAsync(app);
            return app;
        }

        existing.Name = draft.Name;
        if (!string.IsNullOrWhiteSpace(draft.Description))
        {
            existing.Description = draft.Description;
        }
        existing.Modules = modules;
        existing.SyncHint = draft.SyncHint;
        await _apps.UpdateAsync(existing);
        return existing;
    }
}
