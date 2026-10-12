using LiteDB;

namespace AppPortal.Models;

public class AppModule
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsEnabled { get; set; } = true;
    public int Order { get; set; }
    public string? SourceService { get; set; }

    /// <summary>Host for this module's endpoints.</summary>
    public ObjectId? HostId { get; set; }

    /// <summary>http or https — applies to all ports in the module.</summary>
    public string Scheme { get; set; } = "http";

    /// <summary>Single path for the module, e.g. /admin.</summary>
    public string? Path { get; set; }

    public List<AppPort> Ports { get; set; } = new();
}
