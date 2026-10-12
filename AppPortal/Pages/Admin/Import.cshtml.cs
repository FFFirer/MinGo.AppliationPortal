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

    public ImportModel(IDataSourceService sources, IImportService import)
    {
        _sources = sources;
        _import = import;
    }

    public DataSource? Source { get; set; }
    public List<ExternalCandidate> Candidates { get; set; } = new();
    public AppDraft? Draft { get; set; }
    public string? Message { get; set; }
    public string? Error { get; set; }

    [BindProperty]
    public string? SelectedExternalId { get; set; }

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

    public async Task<IActionResult> OnPostImportAsync(string sourceId)
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
            // Apply posted selection checkboxes if present
            ApplySelection();

            var app = await _import.ImportDraftAsync(Source, Draft);
            // Update source LastStatus
            await _sources.UpdateAsync(Source);
            return Redirect($"/Detail/{app.Id}");
        }
        catch (Exception ex)
        {
            Error = $"导入失败：{ex.Message}";
            return Page();
        }
    }

    private void ApplySelection()
    {
        if (Draft is null)
        {
            return;
        }

        // Keep defaults from ResolveAsync; selection form fields named Selected_{moduleIndex}_{epIndex}
        for (var mi = 0; mi < Draft.Modules.Count; mi++)
        {
            for (var ei = 0; ei < Draft.Modules[mi].Endpoints.Count; ei++)
            {
                var key = $"Selected_{mi}_{ei}";
                if (Request.Form.ContainsKey(key))
                {
                    Draft.Modules[mi].Endpoints[ei].Selected = Request.Form[key] == "true";
                }
            }

            var nameKey = $"ModuleName_{mi}";
            if (Request.Form.ContainsKey(nameKey) && !string.IsNullOrWhiteSpace(Request.Form[nameKey]))
            {
                Draft.Modules[mi].Name = Request.Form[nameKey].ToString();
            }
        }
    }
}
