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
            Addresses = new List<string> { string.Empty }
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
                    Addresses = (m.Addresses ?? new List<string>())
                        .Select(a => a?.Trim() ?? "")
                        .Where(a => a.Length > 0)
                        .Where(IsValidUrl)
                        .ToList()
                })
                .Where(m => m.Addresses.Count > 0)
                .ToList()
        };
    }

    public static bool IsValidUrl(string url)
    {
        return url.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || url.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
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
                    Addresses = m.Addresses.Count > 0 ? m.Addresses.ToList() : new List<string> { string.Empty }
                })
                .ToList()
        };
    }
}
