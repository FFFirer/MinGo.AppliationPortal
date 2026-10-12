using AppPortal.Models;

namespace AppPortal.Services;

public sealed record ResolvedEndpoint(
    AppEndpoint Endpoint,
    string? Url,
    string DisplayText,
    bool IsReachable,
    bool IsClickable);

public static class UrlResolver
{
    public static ResolvedEndpoint Resolve(AppEndpoint endpoint, AccessHost? host)
    {
        if (!string.IsNullOrWhiteSpace(endpoint.UrlOverride))
        {
            var overrideUrl = endpoint.UrlOverride.Trim();
            return new ResolvedEndpoint(endpoint, overrideUrl, overrideUrl, true, true);
        }

        if (host is null || string.IsNullOrWhiteSpace(host.Address))
        {
            return new ResolvedEndpoint(endpoint, null, "当前网络不可达", false, false);
        }

        if (endpoint.HostId is { } pinned && host.Id != pinned)
        {
            return new ResolvedEndpoint(endpoint, null, "当前网络不可达", false, false);
        }

        var address = host.Address.Trim();
        var scheme = NormalizeScheme(endpoint.Scheme);
        var path = NormalizePath(endpoint.Path);
        var portSegment = BuildPortSegment(scheme, endpoint.Port);
        var url = $"{scheme}://{address}{portSegment}{path}";
        return new ResolvedEndpoint(endpoint, url, url, true, true);
    }

    private static string NormalizeScheme(string? scheme)
    {
        return string.Equals(scheme, "https", StringComparison.OrdinalIgnoreCase) ? "https" : "http";
    }

    private static string BuildPortSegment(string scheme, int? port)
    {
        if (port is null)
        {
            return "";
        }

        if (scheme == "https" && port == 443)
        {
            return "";
        }

        if (scheme == "http" && port == 80)
        {
            return "";
        }

        return $":{port}";
    }

    private static string NormalizePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return "";
        }

        var p = path.Trim();
        if (!p.StartsWith('/'))
        {
            p = "/" + p;
        }

        return p.TrimEnd('/') == "" ? "" : p;
    }
}
