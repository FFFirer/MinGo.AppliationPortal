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
        foreach (var app in apps)
        {
            AppMigration.EnsureModules(app);
        }
        return Task.FromResult(apps);
    }

    public Task<List<AppEntry>> GetEnabledAppsAsync()
    {
        var apps = _apps.Find(a => a.IsEnabled).OrderBy(a => a.SortOrder).ToList();
        foreach (var app in apps)
        {
            AppMigration.EnsureModules(app);
        }
        return Task.FromResult(apps);
    }

    public Task<AppEntry?> GetByIdAsync(ObjectId id)
    {
        var app = _apps.FindById(id);
        if (app is not null)
        {
            AppMigration.EnsureModules(app);
        }
        return Task.FromResult<AppEntry?>(app);
    }

    public Task CreateAsync(AppEntry app)
    {
        AppMigration.EnsureModules(app);
        app.CreatedAt = DateTime.UtcNow;
        app.UpdatedAt = DateTime.UtcNow;
        _apps.Insert(app);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(AppEntry app)
    {
        AppMigration.EnsureModules(app);
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
            AppMigration.EnsureModules(app);
            app.IsEnabled = !app.IsEnabled;
            app.UpdatedAt = DateTime.UtcNow;
            _apps.Update(app);
        }
        return Task.CompletedTask;
    }
}

internal static class AppMigration
{
    public static void EnsureModules(AppEntry app)
    {
        if (app.Modules.Count > 0)
        {
            return;
        }

        if (app.Urls is not { Count: > 0 })
        {
            app.Modules = new List<AppModule>();
            return;
        }

        var module = new AppModule
        {
            Name = "访问入口",
            IsEnabled = true,
            Order = 0,
            Ports = app.Urls
                .Where(u => !string.IsNullOrWhiteSpace(u.Url))
                .Select((u, idx) => new AppPort
                {
                    Port = 80 + idx,
                    UrlOverride = u.Url.Trim()
                })
                .ToList()
        };

        app.Modules = new List<AppModule> { module };
    }
}
