namespace AppPortal.Models;

public enum EndpointKind
{
    Http = 0,
    Tcp = 1,
    Udp = 2
}

public class AppEndpoint
{
    public string DisplayName { get; set; } = string.Empty;
    public EndpointKind Kind { get; set; } = EndpointKind.Http;
    public LiteDB.ObjectId? HostId { get; set; }
    public int? Port { get; set; }
    public int? ContainerPort { get; set; }
    public string? Path { get; set; }
    public string? SchemeOverride { get; set; }
    public string? UrlOverride { get; set; }
}
