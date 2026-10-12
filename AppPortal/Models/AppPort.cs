namespace AppPortal.Models;

/// <summary>A single published port under a module. Display label is the port number itself.</summary>
public class AppPort
{
    public int Port { get; set; }
    public int? ContainerPort { get; set; }
    public string? UrlOverride { get; set; }
}
