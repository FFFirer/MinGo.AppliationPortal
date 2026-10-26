using LiteDB;

namespace AppPortal.Models;

/// <summary>Portainer 上的一个 Environment（Endpoint），由数据源自动同步产生。</summary>
public class PortainerEnvironment
{
    public ObjectId Id { get; set; } = ObjectId.NewObjectId();

    /// <summary>所属数据源（Portainer 实例）。</summary>
    public ObjectId DataSourceId { get; set; } = ObjectId.Empty;

    /// <summary>Portainer Endpoint 数字 Id，Docker API 路径使用。</summary>
    public int EndpointId { get; set; }

    /// <summary>展示名，同步自 Portainer。</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Endpoint 类型展示名（Docker / Agent / Kubernetes 等）。</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>绑定的 AccessHost；空 = 未绑定，导入回落全量。</summary>
    public List<ObjectId> HostIds { get; set; } = new();

    public DateTime? LastSyncAt { get; set; }
}
