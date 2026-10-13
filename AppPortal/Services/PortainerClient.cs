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
                GetInt32(item, "Id"),
                GetString(item, "Name"),
                GetInt32(item, "EndpointId"),
                FormatStackStatus(item),
                GetInt32(item, "Type", defaultValue: 2)));
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
                    labels[prop.Name] = GetString(prop.Value);
                }
            }

            var project = labels.GetValueOrDefault("com.docker.compose.project");
            if (!string.IsNullOrEmpty(project) &&
                !string.Equals(project, stack.Name, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var service = labels.GetValueOrDefault("com.docker.compose.service");
            var containerName = "";
            if (container.TryGetProperty("Names", out var names) && names.ValueKind == JsonValueKind.Array)
            {
                foreach (var nameEl in names.EnumerateArray())
                {
                    var n = GetString(nameEl).TrimStart('/');
                    if (!string.IsNullOrEmpty(n))
                    {
                        containerName = n;
                        break;
                    }
                }
            }

            if (!container.TryGetProperty("Ports", out var portsEl) || portsEl.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            foreach (var port in portsEl.EnumerateArray())
            {
                var privatePort = GetInt32(port, "PrivatePort");
                var publicPort = port.TryGetProperty("PublicPort", out var pub) && pub.ValueKind == JsonValueKind.Number
                    ? pub.GetInt32()
                    : (int?)null;
                var type = GetString(port, "Type");
                if (string.IsNullOrEmpty(type))
                {
                    type = "tcp";
                }

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

    // Portainer stack Status 等字段可能是 Number，也可能是 String，需兼容读取。
    private static string GetString(JsonElement el, string name)
        => el.TryGetProperty(name, out var value) ? GetString(value) : "";

    private static string GetString(JsonElement value)
    {
        switch (value.ValueKind)
        {
            case JsonValueKind.String:
                return value.GetString() ?? "";
            case JsonValueKind.Number:
                return value.GetRawText();
            case JsonValueKind.True:
                return "true";
            case JsonValueKind.False:
                return "false";
            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
                return "";
            default:
                return value.GetRawText();
        }
    }

    private static int GetInt32(JsonElement el, string name, int defaultValue = 0)
    {
        if (!el.TryGetProperty(name, out var value))
        {
            return defaultValue;
        }

        return GetInt32(value, defaultValue);
    }

    private static int GetInt32(JsonElement value, int defaultValue = 0)
    {
        switch (value.ValueKind)
        {
            case JsonValueKind.Number:
                return value.TryGetInt32(out var n) ? n : defaultValue;
            case JsonValueKind.String:
                return int.TryParse(value.GetString(), out var parsed) ? parsed : defaultValue;
            default:
                return defaultValue;
        }
    }

    private static string FormatStackStatus(JsonElement item)
    {
        if (!item.TryGetProperty("Status", out var status))
        {
            return "";
        }

        if (status.ValueKind == JsonValueKind.Number && status.TryGetInt32(out var code))
        {
            return code switch
            {
                1 => "active",
                2 => "inactive",
                3 => "removed",
                _ => code.ToString()
            };
        }

        return GetString(status);
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
        var token = doc.RootElement.TryGetProperty("jwt", out var jwt) ? GetString(jwt) : "";
        return string.IsNullOrEmpty(token) ? null : token;
    }
}
