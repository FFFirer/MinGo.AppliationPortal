using LiteDB;

namespace AppPortal.Models;

public class AppModule
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsEnabled { get; set; } = true;
    public int Order { get; set; }
    public string? SourceService { get; set; }

    /// <summary>One URL row per address; host is chosen inside each row.</summary>
    public List<ModuleAddress> Addresses { get; set; } = new();
}

/// <summary>
/// One access address under a module, laid out as a URL:
/// [Host] scheme://address[:port][/path]. Port and path are optional.
/// </summary>
public class ModuleAddress
{
    /// <summary>Optional host group this address belongs to.</summary>
    public ObjectId? HostId { get; set; }

    /// <summary>http or https</summary>
    public string Scheme { get; set; } = "http";

    /// <summary>Host IP or name.</summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>Optional port.</summary>
    public int? Port { get; set; }

    /// <summary>Optional path, e.g. /admin</summary>
    public string? Path { get; set; }
}
