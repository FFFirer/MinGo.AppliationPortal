namespace AppPortal.Services;

/// <summary>URL helpers for module address strings (scheme required).</summary>
public static class UrlResolver
{
    public static bool IsValidUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return false;
        }

        var u = url.Trim();
        return u.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || u.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
    }

    public static string Build(string scheme, string address, string? port, string? path)
    {
        var s = string.Equals(scheme, "https", StringComparison.OrdinalIgnoreCase) ? "https" : "http";
        var host = (address ?? "").Trim();
        var portSeg = !string.IsNullOrWhiteSpace(port) ? ":" + port.Trim().TrimStart(':') : "";
        var p = (path ?? "").Trim();
        if (p.Length > 0 && !p.StartsWith('/'))
        {
            p = "/" + p;
        }

        return $"{s}://{host}{portSeg}{p}";
    }
}
