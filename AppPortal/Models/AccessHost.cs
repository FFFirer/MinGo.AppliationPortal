using LiteDB;

namespace AppPortal.Models;

/// <summary>Named host group that can hold several reachable addresses (IPs / names).</summary>
public class AccessHost
{
    public ObjectId Id { get; set; } = ObjectId.NewObjectId();
    public string Name { get; set; } = string.Empty;
    public List<string> Addresses { get; set; } = new();
    public bool IsDefault { get; set; }
    public int SortOrder { get; set; }
}
