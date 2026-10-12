using AppPortal.Models;
using LiteDB;

namespace AppPortal.Services;

public class HostService : IHostService
{
    private readonly ILiteCollection<AccessHost> _hosts;

    public HostService(LiteDatabase db)
    {
        _hosts = db.GetCollection<AccessHost>("hosts");
        _hosts.EnsureIndex(x => x.Name);
    }

    public Task<List<AccessHost>> GetAllAsync()
        => Task.FromResult(_hosts.FindAll().OrderBy(h => h.SortOrder).ThenBy(h => h.Name).ToList());

    public Task<AccessHost?> GetByIdAsync(ObjectId id)
        => Task.FromResult(_hosts.FindById(id));

    public Task CreateAsync(AccessHost host)
    {
        _hosts.Insert(host);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(AccessHost host)
    {
        _hosts.Update(host);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(ObjectId id)
    {
        _hosts.Delete(id);
        return Task.CompletedTask;
    }

    public async Task<List<AccessHost>> ResolveForAppAsync(AppEntry app)
    {
        var all = await GetAllAsync();
        if (app.HostRefs.Count == 0)
        {
            return all;
        }

        var ids = app.HostRefs.Select(r => r.HostId).ToHashSet();
        var list = all.Where(h => ids.Contains(h.Id)).ToList();

        var preferred = app.HostRefs.FirstOrDefault(r => r.IsPreferred);
        if (preferred is not null)
        {
            list = list
                .OrderByDescending(h => h.Id == preferred.HostId)
                .ThenBy(h => h.SortOrder)
                .ThenBy(h => h.Name)
                .ToList();
        }

        return list;
    }
}
