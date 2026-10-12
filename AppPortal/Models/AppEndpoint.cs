namespace AppPortal.Models;

public class AppEndpoint
{
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>http or https — set when configuring the application.</summary>
    public string Scheme { get; set; } = "http";

    public LiteDB.ObjectId? HostId { get; set; }
    public int? Port { get; set; }
    public int? ContainerPort { get; set; }
    public string? Path { get; set; }
    public string? UrlOverride { get; set; }
}
