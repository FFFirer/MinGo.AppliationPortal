using AppPortal.Models;
using AppPortal.Services;
using LiteDB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AppPortal.Pages.Admin;

public class ImportModel : PageModel
{
    private readonly IDataSourceService _sources;
    private readonly IImportService _import;
    private readonly IHostService _hosts;

    public ImportModel(IDataSourceService sources, IImportService import, IHostService hosts)
    {
        _sources = sources;
        _import = import;
        _hosts = hosts;
    }

    public DataSource? Source { get; set; }
    public List<ExternalCandidate> Candidates { get; set; } = new();
    public List<AccessHost> Hosts { get; set; } = new();
    public AppDraft? Draft { get; set; }
    public string? Message { get; set; }
    public string? Error { get; set; }

    [BindProperty]
    public string? SelectedExternalId { get; set; }

    /// <summary>Batch rewrite helper: scheme / host / path for all address URLs.</summary>
    [BindProperty]
    public string GenScheme { get; set; } = "http";

    /// <summary>Selected AccessHost Id; rewrite expands to every address on that host.</summary>
    [BindProperty]
    public string? GenHostId { get; set; }

    [BindProperty]
    public string? GenPath { get; set; }

    public async Task<IActionResult> OnGetAsync(string sourceId, string? externalId)
    {
        if (!IdParser.TryParse(sourceId, out var oid))
        {
            return NotFound();
        }

        Source = await _sources.GetByIdAsync(oid);
        if (Source is null)
        {
            return NotFound();
        }

        Hosts = await _hosts.GetAllAsync();
        GenHostId = ResolveDefaultHostId(Hosts);

        try
        {
            Candidates = await _import.ListCandidatesAsync(Source);
        }
        catch (Exception ex)
        {
            Error = $"拉取候选失败：{ex.Message}";
            return Page();
        }

        if (!string.IsNullOrWhiteSpace(externalId))
        {
            SelectedExternalId = externalId;
            var candidate = Candidates.FirstOrDefault(c => c.ExternalId == externalId)
                ?? Candidates.FirstOrDefault(c => string.Equals(c.Name, externalId, StringComparison.OrdinalIgnoreCase));
            if (candidate is not null)
            {
                try
                {
                    Draft = await _import.ResolveDraftAsync(Source, candidate);
                    SelectedExternalId = Draft.ExternalId ?? candidate.ExternalId;
                    RewriteUrlsFromPortLabels();
                }
                catch (Exception ex)
                {
                    Error = $"解析失败：{ex.Message}";
                }
            }
        }

        return Page();
    }

    public async Task<IActionResult> OnPostRewriteAsync(string sourceId)
    {
        return await LoadDraftAndApplyAsync(sourceId, rewriteOnly: true);
    }

    public async Task<IActionResult> OnPostImportAsync(string sourceId)
    {
        return await LoadDraftAndApplyAsync(sourceId, rewriteOnly: false);
    }

    private async Task<IActionResult> LoadDraftAndApplyAsync(string sourceId, bool rewriteOnly)
    {
        if (!IdParser.TryParse(sourceId, out var oid))
        {
            return NotFound();
        }

        Source = await _sources.GetByIdAsync(oid);
        if (Source is null)
        {
            return NotFound();
        }

        Hosts = await _hosts.GetAllAsync();
        if (string.IsNullOrWhiteSpace(GenHostId))
        {
            GenHostId = ResolveDefaultHostId(Hosts);
        }

        try
        {
            Candidates = await _import.ListCandidatesAsync(Source);
            var candidate = Candidates.FirstOrDefault(c => c.ExternalId == SelectedExternalId)
                ?? Candidates.FirstOrDefault(c => string.Equals(c.Name, SelectedExternalId, StringComparison.OrdinalIgnoreCase));
            if (candidate is null)
            {
                Error = "未选择有效 stack";
                return Page();
            }

            Draft = await _import.ResolveDraftAsync(Source, candidate);
            RewriteUrlsFromPortLabels();
            if (!rewriteOnly)
            {
                ApplyPostedAddresses();
            }

            if (rewriteOnly)
            {
                return Page();
            }

            var app = await _import.ImportDraftAsync(Source, Draft);
            await _sources.UpdateAsync(Source);
            return Redirect($"/Detail/{app.Id}");
        }
        catch (Exception ex)
        {
            Error = rewriteOnly ? $"更新预览失败：{ex.Message}" : $"导入失败：{ex.Message}";
            return Page();
        }
    }

    private static string? ResolveDefaultHostId(List<AccessHost> hosts)
    {
        var ordered = hosts.OrderBy(h => h.SortOrder).ThenBy(h => h.Name).ToList();
        var pick = ordered.FirstOrDefault(h => h.IsDefault) ?? ordered.FirstOrDefault();
        return pick?.Id.ToString();
    }

    private void ApplyPostedAddresses()
    {
        if (Draft is null)
        {
            return;
        }

        for (var mi = 0; mi < Draft.Modules.Count; mi++)
        {
            for (var ai = 0; ai < Draft.Modules[mi].Addresses.Count; ai++)
            {
                var addr = Draft.Modules[mi].Addresses[ai];
                var selKey = $"Sel_{mi}_{ai}";
                // Unchecked checkboxes are omitted from the form post.
                addr.Selected = Request.Form.ContainsKey(selKey);

                var urlKey = $"Url_{mi}_{ai}";
                if (Request.Form.ContainsKey(urlKey))
                {
                    addr.Url = Request.Form[urlKey].ToString();
                }
            }
        }
    }

    /// <summary>
    /// Rebuild address list from Portainer labels like 8080→80.
    /// Expands each port to every IP on the selected host (all selected by default).
    /// </summary>
    private void RewriteUrlsFromPortLabels()
    {
        if (Draft is null)
        {
            return;
        }

        var host = Hosts.FirstOrDefault(h => h.Id.ToString() == GenHostId)
            ?? Hosts.FirstOrDefault(h => string.Equals(h.Name, GenHostId, StringComparison.OrdinalIgnoreCase));
        var ips = host?.Addresses
            .Where(a => !string.IsNullOrWhiteSpace(a))
            .Select(a => a.Trim().TrimEnd('/'))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList() ?? new List<string>();
        if (ips.Count == 0)
        {
            return;
        }

        var scheme = string.Equals(GenScheme, "https", StringComparison.OrdinalIgnoreCase) ? "https" : "http";
        var path = (GenPath ?? "").Trim();
        if (path.Length > 0 && !path.StartsWith('/'))
        {
            path = "/" + path;
        }

        foreach (var module in Draft.Modules)
        {
            var templates = module.Addresses
                .GroupBy(a => a.Label ?? a.Url)
                .Select(g => g.First())
                .ToList();

            var expanded = new List<AppDraftAddress>();
            foreach (var template in templates)
            {
                var label = template.Label ?? "";
                var left = label.Split('→', '-', ':')[0].Trim();
                if (!int.TryParse(left, out var port) || port <= 0)
                {
                    // Keep hand-edited URLs without a parseable port as-is.
                    expanded.Add(new AppDraftAddress
                    {
                        Url = template.Url,
                        Label = template.Label,
                        Selected = template.Selected
                    });
                    continue;
                }

                foreach (var ip in ips)
                {
                    var portSeg = port is 80 && scheme == "http" || port is 443 && scheme == "https"
                        ? ""
                        : $":{port}";
                    expanded.Add(new AppDraftAddress
                    {
                        Url = $"{scheme}://{ip}{portSeg}{path}",
                        Label = template.Label,
                        Selected = true
                    });
                }
            }

            module.Addresses = expanded;
        }
    }
}
