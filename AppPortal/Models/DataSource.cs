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

    // Host 绑定与同步状态已下沉到 PortainerEnvironment；
    // 旧数据中的 HostIds / LastSyncAt / LastStatus 字段由 DataMigration 一次性迁移。
}
