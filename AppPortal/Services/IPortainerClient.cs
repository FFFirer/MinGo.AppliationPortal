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

public sealed record PortainerPortMapping(
    string? ContainerName,
    string? ServiceName,
    int? PublicPort,
    int PrivatePort,
    string Type);

public interface IPortainerClient
{
    Task<bool> TestAsync(string configJson);
    Task<List<PortainerStackInfo>> ListStacksAsync(string configJson);
    Task<List<PortainerPortMapping>> GetStackPortsAsync(string configJson, PortainerStackInfo stack);
}
