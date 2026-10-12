namespace AppPortal.Models;

public class AppModule
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsEnabled { get; set; } = true;
    public int Order { get; set; }
    public string? SourceService { get; set; }
    public List<AppEndpoint> Endpoints { get; set; } = new();
}
