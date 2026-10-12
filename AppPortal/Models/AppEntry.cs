using LiteDB;

namespace AppPortal.Models;

public class AppEntry
{
    public ObjectId Id { get; set; } = ObjectId.NewObjectId();
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconUrl { get; set; }
    public List<AppModule> Modules { get; set; } = new();
    public List<HostRef> HostRefs { get; set; } = new();
    public ObjectId? SourceId { get; set; }
    public string? ExternalId { get; set; }
    public string? SyncHint { get; set; }
    public bool IsEnabled { get; set; } = true;
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    /// <summary>Legacy flat URLs; migrated into Modules on read.</summary>
    public List<AppUrl>? Urls { get; set; }
}
