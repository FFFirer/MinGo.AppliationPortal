using AppPortal.Models;
using AppPortal.Services;
using LiteDB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AppPortal.Pages.Admin;

public class EditModel : PageModel
{
    private readonly IAppService _appService;
    private readonly IHostService _hostService;

    [BindProperty]
    public AppEditModel App { get; set; } = new();

    public List<AccessHost> Hosts { get; set; } = new();
    public List<ObjectId> SelectedHostIds { get; set; } = new();

    public EditModel(IAppService appService, IHostService hostService)
    {
        _appService = appService;
        _hostService = hostService;
    }

    public async Task<IActionResult> OnGetAsync(string id)
    {
        if (!IdParser.TryParse(id, out var oid))
        {
            return NotFound();
        }

        var app = await _appService.GetByIdAsync(oid);
        if (app is null)
        {
            return NotFound();
        }

        App = AppEditModel.FromEntry(app);
        Hosts = await _hostService.GetAllAsync();
        SelectedHostIds = App.HostRefs.Select(r => r.HostId).ToList();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string id)
    {
        if (!IdParser.TryParse(id, out var oid))
        {
            return NotFound();
        }

        Hosts = await _hostService.GetAllAsync();
        SelectedHostIds = App.HostRefs.Select(r => r.HostId).ToList();

        if (string.IsNullOrWhiteSpace(App.Name))
        {
            ModelState.AddModelError("App.Name", "应用名称不能为空");
        }

        var entry = AppEditModel.ToEntry(App, oid);
        entry.HostRefs = SelectedHostIds
            .Distinct()
            .Select(id => new HostRef { HostId = id })
            .ToList();
        if (entry.HostRefs.Count > 0)
        {
            entry.HostRefs[0].IsPreferred = true;
        }

        if (entry.Modules.Count == 0)
        {
            ModelState.AddModelError("App.Modules", "至少需要一个模块下的一个有效入口");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Preserve source provenance
        var existing = await _appService.GetByIdAsync(oid);
        if (existing is not null)
        {
            entry.SourceId = existing.SourceId;
            entry.ExternalId = existing.ExternalId;
            entry.SyncHint = existing.SyncHint;
            entry.CreatedAt = existing.CreatedAt;
        }

        await _appService.UpdateAsync(entry);
        return RedirectToPage("Index");
    }
}
