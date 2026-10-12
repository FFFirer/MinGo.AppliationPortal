namespace AppPortal.Models;

public class AppModule
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsEnabled { get; set; } = true;
    public int Order { get; set; }
    public string? SourceService { get; set; }

    /// <summary>Full URL strings, e.g. http://10.0.0.5:8080/admin. Scheme is required.</summary>
    public List<string> Addresses { get; set; } = new();
}
