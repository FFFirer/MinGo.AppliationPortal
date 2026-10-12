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

    [BindProperty]
    public string? GenHost { get; set; }

    [BindProperty]
    public string? GenHostCustom { get; set; }

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
        GenHost = Hosts.SelectMany(h => h.Addresses).FirstOrDefault();

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
            var candidate = Candidates.FirstOrDefault(c => c.ExternalId == externalId);
            if (candidate is not null)
            {
                try
                {
                    Draft = await _import.ResolveDraftAsync(Source, candidate);
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

        try
        {
            Candidates = await _import.ListCandidatesAsync(Source);
            var candidate = Candidates.FirstOrDefault(c => c.ExternalId == SelectedExternalId);
            if (candidate is null)
            {
                Error = "未选择有效 stack";
                return Page();
            }

            Draft = await _import.ResolveDraftAsync(Source, candidate);
            ApplyPostedAddresses();
            RewriteUrlsFromPortLabels();

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
                if (Request.Form.ContainsKey(selKey))
                {
                    addr.Selected = Request.Form[selKey] == "true";
                }

                var urlKey = $"Url_{mi}_{ai}";
                if (Request.Form.ContainsKey(urlKey))
                {
                    addr.Url = Request.Form[urlKey].ToString();
                }
            }
        }
    }

    /// <summary>
    /// Rebuild URL strings from Portainer labels like 8080→80 using selected host/scheme/path.
    /// Keeps hand-edited full URLs that no longer look like placeholders.
    /// </summary>
    private void RewriteUrlsFromPortLabels()
    {
        if (Draft is null)
        {
            return;
        }

        var scheme = string.Equals(GenScheme, "https", StringComparison.OrdinalIgnoreCase) ? "https" : "http";
        var host = (!string.IsNullOrWhiteSpace(GenHostCustom) ? GenHostCustom : GenHost ?? "").Trim().TrimEnd('/');
        if (string.IsNullOrWhiteSpace(host))
        {
            return;
        }

        var path = (GenPath ?? "").Trim();
        if (path.Length > 0 && !path.StartsWith('/'))
        {
            path = "/" + path;
        }

        foreach (var module in Draft.Modules)
        {
            foreach (var addr in module.Addresses)
            {
                var label = addr.Label ?? "";
                // Label is "8080→80" or "8080" or "80"
                var left = label.Split('→', '-', ':')[0].Trim();
                if (int.TryParse(left, out var port) && port > 0)
                {
                    var portSeg = port is 80 && scheme == "http" || port is 443 && scheme == "https" ? "" : $":{port}";
                    addr.Url = $"{scheme}://{host}{portSeg}{path}";
                }
            }
        }
    }
}
