using AppPortal.Models;
using AppPortal.Services;
using LiteDB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AppPortal.Pages.Admin;

public class ImportModel : PageModel
{
    private readonly IDataSourceService _sources;
    private readonly IEnvironmentService _environments;
    private readonly IImportService _import;
    private readonly IHostService _hosts;

    public ImportModel(IDataSourceService sources, IEnvironmentService environments, IImportService import, IHostService hosts)
    {
        _sources = sources;
        _environments = environments;
        _import = import;
        _hosts = hosts;
    }

    public DataSource? Source { get; set; }
    public PortainerEnvironment? Environment { get; set; }
    public List<ExternalCandidate> Candidates { get; set; } = new();
    public List<AccessHost> Hosts { get; set; } = new();
    public AppDraft? Draft { get; set; }
    public string? Message { get; set; }
    public string? Error { get; set; }

    /// <summary>True when Environment.HostIds is non-empty but none resolve to a live Host.</summary>
    public bool HostBindingStale { get; set; }

    [BindProperty]
    public string? SelectedExternalId { get; set; }

    /// <summary>Batch rewrite helper: scheme / host / path for all address URLs.</summary>
    [BindProperty]
    public string GenScheme { get; set; } = "http";

    /// <summary>Selected AccessHost Ids; rewrite expands to every address on those hosts.</summary>
    [BindProperty]
    public List<string> GenHostIds { get; set; } = new();

    [BindProperty]
    public string? GenPath { get; set; }

    public async Task<IActionResult> OnGetAsync(string sourceId, string? environmentId, string? externalId)
    {
        if (!await LoadSourceAsync(sourceId))
        {
            return NotFound();
        }

        if (!await LoadEnvironmentAsync(environmentId))
        {
            return Page();
        }

        LoadHostCandidates(await _hosts.GetAllAsync());
        GenHostIds = Hosts.Select(h => h.Id.ToString()).ToList();

        try
        {
            Candidates = await _import.ListCandidatesAsync(Source!, Environment!.EndpointId);
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
                    Draft = await _import.ResolveDraftAsync(Source!, Environment!.EndpointId, candidate);
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

    public async Task<IActionResult> OnPostRewriteAsync(string sourceId, string? environmentId)
    {
        return await LoadDraftAndApplyAsync(sourceId, environmentId, rewriteOnly: true);
    }

    public async Task<IActionResult> OnPostImportAsync(string sourceId, string? environmentId)
    {
        return await LoadDraftAndApplyAsync(sourceId, environmentId, rewriteOnly: false);
    }

    private async Task<IActionResult> LoadDraftAndApplyAsync(string sourceId, string? environmentId, bool rewriteOnly)
    {
        if (!await LoadSourceAsync(sourceId))
        {
            return NotFound();
        }

        if (!await LoadEnvironmentAsync(environmentId))
        {
            return Page();
        }

        LoadHostCandidates(await _hosts.GetAllAsync());
        if (GenHostIds.Count == 0)
        {
            GenHostIds = Hosts.Select(h => h.Id.ToString()).ToList();
        }
        else
        {
            // Drop posts that are no longer in the candidate pool (e.g. stale binding).
            var allowed = Hosts.Select(h => h.Id.ToString()).ToHashSet(StringComparer.OrdinalIgnoreCase);
            GenHostIds = GenHostIds.Where(id => allowed.Contains(id)).ToList();
            if (GenHostIds.Count == 0)
            {
                GenHostIds = Hosts.Select(h => h.Id.ToString()).ToList();
            }
        }

        try
        {
            Candidates = await _import.ListCandidatesAsync(Source!, Environment!.EndpointId);
            var candidate = Candidates.FirstOrDefault(c => c.ExternalId == SelectedExternalId)
                ?? Candidates.FirstOrDefault(c => string.Equals(c.Name, SelectedExternalId, StringComparison.OrdinalIgnoreCase));
            if (candidate is null)
            {
                Error = "未选择有效的 compose project";
                return Page();
            }

            Draft = await _import.ResolveDraftAsync(Source!, Environment!.EndpointId, candidate);
            RewriteUrlsFromPortLabels();
            if (!rewriteOnly)
            {
                ApplyPostedAddresses();
            }

            if (rewriteOnly)
            {
                return Page();
            }

            var app = await _import.ImportDraftAsync(Source!, Environment!.Id, Draft);
            return Redirect($"/Detail/{app.Id}");
        }
        catch (Exception ex)
        {
            Error = rewriteOnly ? $"更新预览失败：{ex.Message}" : $"导入失败：{ex.Message}";
            return Page();
        }
    }

    private async Task<bool> LoadSourceAsync(string sourceId)
    {
        if (!IdParser.TryParse(sourceId, out var oid))
        {
            return false;
        }

        Source = await _sources.GetByIdAsync(oid);
        return Source is not null;
    }

    /// <summary>解析指定 Environment；缺省时回落到该数据源的第一个 Environment。</summary>
    private async Task<bool> LoadEnvironmentAsync(string? environmentId)
    {
        if (Source is null)
        {
            return false;
        }

        if (IdParser.TryParse(environmentId, out var oid))
        {
            var env = await _environments.GetByIdAsync(oid);
            if (env is not null && env.DataSourceId == Source.Id)
            {
                Environment = env;
                return true;
            }
        }

        var list = await _environments.GetByDataSourceAsync(Source.Id);
        if (list.Count == 0)
        {
            try
            {
                list = await _environments.SyncFromPortainerAsync(Source);
            }
            catch (Exception ex)
            {
                Error = $"同步 Environment 失败：{ex.Message}";
                return false;
            }
        }

        Environment = list.OrderBy(e => e.EndpointId).FirstOrDefault();
        if (Environment is null)
        {
            Error = "该数据源下暂无 Environment，请先在数据源页同步。";
            return false;
        }

        return true;
    }

    /// <summary>
    /// Host pool for import: bound hosts when the binding resolves; otherwise all hosts.
    /// </summary>
    private void LoadHostCandidates(List<AccessHost> all)
    {
        all = all.OrderBy(h => h.SortOrder).ThenBy(h => h.Name).ToList();
        HostBindingStale = false;

        var boundIds = Environment?.HostIds ?? new List<ObjectId>();
        if (boundIds.Count == 0)
        {
            Hosts = all;
            return;
        }

        var bound = all.Where(h => boundIds.Contains(h.Id)).ToList();
        if (bound.Count == 0)
        {
            HostBindingStale = true;
            Hosts = all;
            return;
        }

        Hosts = bound;
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
    /// Expands each port to the union of IPs on every selected host (all selected by default).
    /// </summary>
    private void RewriteUrlsFromPortLabels()
    {
        if (Draft is null)
        {
            return;
        }

        var selectedIds = new HashSet<string>(GenHostIds, StringComparer.OrdinalIgnoreCase);
        var ips = Hosts
            .Where(h => selectedIds.Count == 0 || selectedIds.Contains(h.Id.ToString()) || selectedIds.Contains(h.Name))
            .SelectMany(h => h.Addresses)
            .Where(a => !string.IsNullOrWhiteSpace(a))
            .Select(a => a.Trim().TrimEnd('/'))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
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
