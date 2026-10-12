using AppPortal.Models;

namespace AppPortal.Services;

public sealed record ResolvedAddress(ModuleAddress Address, string Url);

public static class UrlResolver
{
    public static ResolvedAddress Resolve(ModuleAddress address)
    {
        var scheme = NormalizeScheme(address.Scheme);
        var host = address.Address.Trim().TrimEnd('/');
        if (string.IsNullOrWhiteSpace(host))
        {
            return new ResolvedAddress(address, "");
        }

        // Allow pasting a full URL into Address
        if (host.Contains("://", StringComparison.Ordinal))
        {
            return new ResolvedAddress(address, host);
        }

        var portSeg = address.Port is { } p && p > 0 ? $":{p}" : "";
        var path = NormalizePath(address.Path);
        var url = $"{scheme}://{host}{portSeg}{path}";
        return new ResolvedAddress(address, url);
    }

    public static string NormalizeScheme(string? scheme)
        => string.Equals(scheme, "https", StringComparison.OrdinalIgnoreCase) ? "https" : "http";

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
