using System.Net.Http.Headers;
using System.Text.Json;

namespace AppPortal.Services;

public class PortainerClient : IPortainerClient
{
    private readonly IHttpClientFactory _httpClientFactory;

    public PortainerClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<bool> TestAsync(string configJson)
    {
        var config = Parse(configJson);
        var client = CreateClient(config);
        var response = await client.GetAsync("/api/stacks");
        return response.IsSuccessStatusCode;
    }

    public async Task<List<PortainerStackInfo>> ListStacksAsync(string configJson)
    {
        var config = Parse(configJson);
        var client = CreateClient(config);
        using var response = await client.GetAsync("/api/stacks");
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);
        var result = new List<PortainerStackInfo>();
        foreach (var item in doc.RootElement.EnumerateArray())
        {
            result.Add(new PortainerStackInfo(
                item.GetProperty("Id").GetInt32(),
                item.TryGetProperty("Name", out var name) ? name.GetString() ?? "" : "",
                item.TryGetProperty("EndpointId", out var ep) ? ep.GetInt32() : 0,
                item.TryGetProperty("Status", out var st) ? st.GetString() ?? "" : "",
                item.TryGetProperty("Type", out var ty) ? ty.GetInt32() : 2));
        }

        return result;
    }

    public async Task<List<PortainerPortMapping>> GetStackPortsAsync(string configJson, PortainerStackInfo stack)
    {
        var config = Parse(configJson);
        var client = CreateClient(config);

        // Prefer runtime containers filtered by compose project label
        var url = $"/api/endpoints/{stack.EndpointId}/docker/containers/json?all=true";
        using var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);

        var mappings = new List<PortainerPortMapping>();
        foreach (var container in doc.RootElement.EnumerateArray())
        {
            var labels = new Dictionary<string, string>();
            if (container.TryGetProperty("Labels", out var labelsEl) && labelsEl.ValueKind == JsonValueKind.Object)
            {
                foreach (var prop in labelsEl.EnumerateObject())
                {
                    labels[prop.Name] = prop.Value.GetString() ?? "";
                }
            }

            var project = labels.GetValueOrDefault("com.docker.compose.project");
            if (!string.IsNullOrEmpty(project) &&
                !string.Equals(project, stack.Name, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var service = labels.GetValueOrDefault("com.docker.compose.service");
            var containerName = container.TryGetProperty("Names", out var names) && names.ValueKind == JsonValueKind.Array
                ? names[0].GetString()?.TrimStart('/') ?? ""
                : "";

            if (!container.TryGetProperty("Ports", out var portsEl) || portsEl.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            foreach (var port in portsEl.EnumerateArray())
            {
                var privatePort = port.TryGetProperty("PrivatePort", out var pp) ? pp.GetInt32() : 0;
                var publicPort = port.TryGetProperty("PublicPort", out var pub) && pub.ValueKind == JsonValueKind.Number
                    ? pub.GetInt32()
                    : (int?)null;
                var type = port.TryGetProperty("Type", out var t) ? t.GetString() ?? "tcp" : "tcp";

                if (publicPort is null && privatePort == 0)
                {
                    continue;
                }

                mappings.Add(new PortainerPortMapping(
                    containerName,
                    string.IsNullOrEmpty(service) ? containerName : service,
                    publicPort,
                    privatePort == 0 ? publicPort ?? 0 : privatePort,
                    type));
            }
        }

        return mappings;
    }

    private static PortainerConfig Parse(string configJson)
    {
        if (string.IsNullOrWhiteSpace(configJson))
        {
            throw new InvalidOperationException("Portainer Config 为空");
        }

        var config = JsonSerializer.Deserialize<PortainerConfig>(configJson)
            ?? throw new InvalidOperationException("Portainer Config 解析失败");
        if (string.IsNullOrWhiteSpace(config.BaseUrl))
        {
            throw new InvalidOperationException("Portainer BaseUrl 未配置");
        }

        return config;
    }

    private HttpClient CreateClient(PortainerConfig config)
    {
        var client = _httpClientFactory.CreateClient("portainer");
        client.BaseAddress = new Uri(config.BaseUrl.TrimEnd('/') + "/");
        client.Timeout = TimeSpan.FromSeconds(30);
        if (!string.IsNullOrWhiteSpace(config.ApiKey))
        {
            client.DefaultRequestHeaders.Remove("X-API-Key");
            client.DefaultRequestHeaders.Add("X-API-Key", config.ApiKey);
        }
        else if (!string.IsNullOrWhiteSpace(config.Username))
        {
            // JWT auth fallback: fetch token once per request scope
            var token = AuthenticateAsync(client, config.Username, config.Password ?? "").GetAwaiter().GetResult();
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        return client;
    }

    private static async Task<string?> AuthenticateAsync(HttpClient client, string username, string password)
    {
        var payload = JsonSerializer.Serialize(new { username, password });
        using var content = new StringContent(payload, System.Text.Encoding.UTF8, "application/json");
        using var response = await client.PostAsync("/api/auth", content);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        await using var stream = await response.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);
        return doc.RootElement.TryGetProperty("jwt", out var jwt) ? jwt.GetString() : null;
    }
}
