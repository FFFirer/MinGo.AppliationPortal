using LiteDB;

namespace AppPortal.Models;

/// <summary>Named reachable address used to resolve endpoint URLs.</summary>
public class AccessHost
{
    public ObjectId Id { get; set; } = ObjectId.NewObjectId();
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string SchemeDefault { get; set; } = "http";
    public bool IsDefault { get; set; }
    public int SortOrder { get; set; }
}
