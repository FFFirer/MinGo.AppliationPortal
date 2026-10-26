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
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string id)
    {
        if (!IdParser.TryParse(id, out var oid))
        {
            return NotFound();
        }

        Hosts = await _hostService.GetAllAsync();

        if (string.IsNullOrWhiteSpace(App.Name))
        {
            ModelState.AddModelError("App.Name", "应用名称不能为空");
        }

        var entry = AppEditModel.ToEntry(App, oid);
        if (entry.Modules.Count == 0)
        {
            ModelState.AddModelError("App.Modules", "至少需要一个模块和端口");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var existing = await _appService.GetByIdAsync(oid);
        if (existing is not null)
        {
            entry.SourceId = existing.SourceId;
            entry.EnvironmentId = existing.EnvironmentId;
            entry.ExternalId = existing.ExternalId;
            entry.SyncHint = existing.SyncHint;
            entry.CreatedAt = existing.CreatedAt;
            entry.HostRefs = existing.HostRefs;
        }

        await _appService.UpdateAsync(entry);
        return RedirectToPage("Index");
    }
}
