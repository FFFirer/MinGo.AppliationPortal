using AppPortal.Models;
using LiteDB;

namespace AppPortal.Services;

public class AppService : IAppService
{
    private readonly ILiteCollection<AppEntry> _apps;

    public AppService(LiteDatabase db)
    {
        _apps = db.GetCollection<AppEntry>("apps");
        _apps.EnsureIndex(x => x.Name);
        _apps.EnsureIndex(x => x.IsEnabled);
    }

    public Task<List<AppEntry>> GetAllAppsAsync()
    {
        var apps = _apps.FindAll().OrderBy(a => a.SortOrder).ToList();
        return Task.FromResult(apps);
    }

    public Task<List<AppEntry>> GetEnabledAppsAsync()
    {
        var apps = _apps.Find(a => a.IsEnabled).OrderBy(a => a.SortOrder).ToList();
        return Task.FromResult(apps);
    }

    public Task<AppEntry?> GetByIdAsync(ObjectId id)
    {
        var app = _apps.FindById(id);
        return Task.FromResult<AppEntry?>(app);
    }

    public Task CreateAsync(AppEntry app)
    {
        app.CreatedAt = DateTime.UtcNow;
        app.UpdatedAt = DateTime.UtcNow;
        _apps.Insert(app);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(AppEntry app)
    {
        app.UpdatedAt = DateTime.UtcNow;
        _apps.Update(app);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(ObjectId id)
    {
        _apps.Delete(id);
        return Task.CompletedTask;
    }

    public Task ToggleEnabledAsync(ObjectId id)
    {
        var app = _apps.FindById(id);
        if (app is not null)
        {
            app.IsEnabled = !app.IsEnabled;
            app.UpdatedAt = DateTime.UtcNow;
            _apps.Update(app);
        }
        return Task.CompletedTask;
    }
}
