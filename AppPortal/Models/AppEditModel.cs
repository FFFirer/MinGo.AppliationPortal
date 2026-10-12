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
    public List<AppModule> Modules { get; set; } = new()
    {
        new AppModule
        {
            Name = "访问入口",
            Ports = new List<AppPort> { new() }
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
            Modules = (model.Modules ?? new List<AppModule>())
                .Where(m => !string.IsNullOrWhiteSpace(m.Name))
                .Select((m, i) => new AppModule
                {
                    Name = m.Name.Trim(),
                    Description = string.IsNullOrWhiteSpace(m.Description) ? null : m.Description.Trim(),
                    IsEnabled = m.IsEnabled,
                    Order = m.Order == 0 ? i : m.Order,
                    SourceService = m.SourceService,
                    HostId = m.HostId,
                    Scheme = string.Equals(m.Scheme, "https", StringComparison.OrdinalIgnoreCase) ? "https" : "http",
                    Path = string.IsNullOrWhiteSpace(m.Path) ? null : m.Path.Trim(),
                    Ports = (m.Ports ?? new List<AppPort>())
                        .Where(p => p.Port > 0 || !string.IsNullOrWhiteSpace(p.UrlOverride))
                        .Select(p => new AppPort
                        {
                            Port = p.Port > 0 ? p.Port : 80,
                            ContainerPort = p.ContainerPort,
                            UrlOverride = string.IsNullOrWhiteSpace(p.UrlOverride) ? null : p.UrlOverride.Trim()
                        })
                        .ToList()
                })
                .Where(m => m.Ports.Count > 0)
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
            Modules = entry.Modules
                .Select(m => new AppModule
                {
                    Name = m.Name,
                    Description = m.Description,
                    IsEnabled = m.IsEnabled,
                    Order = m.Order,
                    SourceService = m.SourceService,
                    HostId = m.HostId,
                    Scheme = m.Scheme,
                    Path = m.Path,
                    Ports = m.Ports.ToList()
                })
                .ToList()
        };
    }
}
