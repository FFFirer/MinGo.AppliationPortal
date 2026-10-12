using LiteDB;

namespace AppPortal.Models;

/// <summary>Named reachable address used to resolve endpoint URLs. No scheme here — http/https is chosen on the app entry.</summary>
public class AccessHost
{
    public ObjectId Id { get; set; } = ObjectId.NewObjectId();
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public int SortOrder { get; set; }
}
