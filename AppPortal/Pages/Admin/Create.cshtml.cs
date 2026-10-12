using AppPortal.Models;
using AppPortal.Services;
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

        if (string.IsNullOrWhiteSpace(App.Name))
        {
            ModelState.AddModelError("App.Name", "应用名称不能为空");
        }

        var entry = AppEditModel.ToEntry(App);
        if (entry.Modules.Count == 0)
        {
            ModelState.AddModelError("App.Modules", "至少需要一个模块和端口");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        await _appService.CreateAsync(entry);
        return RedirectToPage("Index");
    }
}
