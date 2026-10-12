using LiteDB;

namespace AppPortal.Models;

public enum DataSourceType
{
    Manual = 0,
    Portainer = 1,
    ComposeFile = 2
}

public class DataSource
{
    public ObjectId Id { get; set; } = ObjectId.NewObjectId();
    public string Name { get; set; } = string.Empty;
    public DataSourceType Type { get; set; } = DataSourceType.Manual;
    public string Config { get; set; } = "{}";
    public bool IsEnabled { get; set; } = true;
    public DateTime? LastSyncAt { get; set; }
    public string? LastStatus { get; set; }
}
