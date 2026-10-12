using AppPortal.Models;
using AppPortal.Services;
using LiteDB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AppPortal.Pages.Admin;

public class CreateModel : PageModel
{
    private readonly IAppService _appService;
    private readonly IHostService _hostService;

    [BindProperty]
    public AppEditModel App { get; set; } = new();

    public List<AccessHost> Hosts { get; set; } = new();
    public List<ObjectId> SelectedHostIds { get; set; } = new();

    public CreateModel(IAppService appService, IHostService hostService)
    {
        _appService = appService;
        _hostService = hostService;
    }

    public async Task OnGetAsync()
    {
        Hosts = await _hostService.GetAllAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Hosts = await _hostService.GetAllAsync();
        SelectedHostIds = App.HostRefs.Select(r => r.HostId).ToList();

        if (string.IsNullOrWhiteSpace(App.Name))
        {
            ModelState.AddModelError("App.Name", "应用名称不能为空");
        }

        var entry = AppEditModel.ToEntry(App);
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

        await _appService.CreateAsync(entry);
        return RedirectToPage("Index");
    }
}
