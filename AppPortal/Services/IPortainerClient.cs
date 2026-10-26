using System.Text.Json.Serialization;

namespace AppPortal.Services;

public class PortainerConfig
{
    [JsonPropertyName("baseUrl")]
    public string BaseUrl { get; set; } = string.Empty;

    [JsonPropertyName("apiKey")]
    public string ApiKey { get; set; } = string.Empty;

    [JsonPropertyName("username")]
    public string? Username { get; set; }

    [JsonPropertyName("password")]
    public string? Password { get; set; }
}

public sealed record PortainerStackInfo(
    int Id,
    string Name,
    int EndpointId,
    string Status,
    int Type);

public sealed record PortainerEndpointInfo(
    int Id,
    string Name,
    string Type);

public sealed record PortainerProjectInfo(
    string Name,
    int EndpointId,
    string Status);

public sealed record PortainerPortMapping(
    string? ContainerName,
    string? ServiceName,
    int? PublicPort,
    int PrivatePort,
    string Type);

/// <summary>
/// Portainer API 底层封装。业务导入走 project 维度；
/// stack 相关方法保留，供存量迁移与后续扩展使用。
/// </summary>
public interface IPortainerClient
{
    Task<bool> TestAsync(string configJson);

    // ---- 业务导入（project 维度） ----

    Task<List<PortainerEndpointInfo>> ListEndpointsAsync(string configJson);
    Task<List<PortainerProjectInfo>> ListProjectsAsync(string configJson, int endpointId);
    Task<List<PortainerPortMapping>> GetProjectPortsAsync(string configJson, int endpointId, string projectName);

    // ---- 底层 stack 封装（迁移用，业务层不再调用） ----

    Task<List<PortainerStackInfo>> ListStacksAsync(string configJson);
    Task<List<PortainerPortMapping>> GetStackPortsAsync(string configJson, PortainerStackInfo stack);
}
