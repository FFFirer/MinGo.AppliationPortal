using AppPortal.Models;

namespace AppPortal.Services;

public sealed record ResolvedPort(
    AppPort Port,
    string? Url,
    string DisplayText,
    bool IsReachable);

public static class UrlResolver
{
    public static ResolvedPort Resolve(AppPort port, AppModule module, AccessHost? host)
    {
        if (!string.IsNullOrWhiteSpace(port.UrlOverride))
        {
            var overrideUrl = port.UrlOverride.Trim();
            return new ResolvedPort(port, overrideUrl, overrideUrl, true);
        }

        if (host is null || string.IsNullOrWhiteSpace(host.Address))
        {
            return new ResolvedPort(port, null, "当前网络不可达", false);
        }

        if (module.HostId is { } pinned && host.Id != pinned)
        {
            return new ResolvedPort(port, null, "当前网络不可达", false);
        }

        var scheme = NormalizeScheme(module.Scheme);
        var address = host.Address.Trim();
        var path = NormalizePath(module.Path);
        var portSegment = BuildPortSegment(scheme, port.Port);
        var url = $"{scheme}://{address}{portSegment}{path}";
        return new ResolvedPort(port, url, url, true);
    }

    public static string NormalizeScheme(string? scheme)
        => string.Equals(scheme, "https", StringComparison.OrdinalIgnoreCase) ? "https" : "http";

    private static string BuildPortSegment(string scheme, int port)
    {
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
