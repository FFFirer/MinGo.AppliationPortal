using AppPortal.Models;
using LiteDB;

namespace AppPortal.Models;

public class AppEditModel
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconUrl { get; set; }
    public int SortOrder { get; set; }
    public bool IsEnabled { get; set; } = true;
    public List<HostRef> HostRefs { get; set; } = new();
    public List<AppModule> Modules { get; set; } = new()
    {
        new AppModule
        {
            Name = "访问入口",
            Endpoints = new List<AppEndpoint> { new() }
        }
    };

    public static AppEntry ToEntry(AppEditModel model, ObjectId? existingId = null)
    {
        return new AppEntry
        {
            Id = existingId ?? ObjectId.NewObjectId(),
            Name = model.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim(),
            IconUrl = string.IsNullOrWhiteSpace(model.IconUrl) ? null : model.IconUrl.Trim(),
            SortOrder = model.SortOrder,
            IsEnabled = model.IsEnabled,
            HostRefs = model.HostRefs ?? new List<HostRef>(),
            Modules = (model.Modules ?? new List<AppModule>())
                .Where(m => !string.IsNullOrWhiteSpace(m.Name))
                .Select((m, i) => new AppModule
                {
                    Name = m.Name.Trim(),
                    Description = string.IsNullOrWhiteSpace(m.Description) ? null : m.Description.Trim(),
                    IsEnabled = m.IsEnabled,
                    Order = m.Order == 0 ? i : m.Order,
                    SourceService = m.SourceService,
                    Endpoints = (m.Endpoints ?? new List<AppEndpoint>())
                        .Where(e => e.Port is not null || !string.IsNullOrWhiteSpace(e.UrlOverride))
                        .Select(e => new AppEndpoint
                        {
                            DisplayName = string.IsNullOrWhiteSpace(e.DisplayName) ? "入口" : e.DisplayName.Trim(),
                            Scheme = string.IsNullOrWhiteSpace(e.Scheme) ? "http" : e.Scheme.Trim().ToLowerInvariant() == "https" ? "https" : "http",
                            HostId = e.HostId,
                            Port = e.Port,
                            ContainerPort = e.ContainerPort,
                            Path = string.IsNullOrWhiteSpace(e.Path) ? null : e.Path.Trim(),
                            UrlOverride = string.IsNullOrWhiteSpace(e.UrlOverride) ? null : e.UrlOverride.Trim()
                        })
                        .ToList()
                })
                .Where(m => m.Endpoints.Count > 0)
                .ToList()
        };
    }

    public static AppEditModel FromEntry(AppEntry entry)
    {
        return new AppEditModel
        {
            Name = entry.Name,
            Description = entry.Description,
            IconUrl = entry.IconUrl,
            SortOrder = entry.SortOrder,
            IsEnabled = entry.IsEnabled,
            HostRefs = entry.HostRefs,
            Modules = entry.Modules
                .Select(m => new AppModule
                {
                    Name = m.Name,
                    Description = m.Description,
                    IsEnabled = m.IsEnabled,
                    Order = m.Order,
                    SourceService = m.SourceService,
                    Endpoints = m.Endpoints.ToList()
                })
                .ToList()
        };
    }
}
