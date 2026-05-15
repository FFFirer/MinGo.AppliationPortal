using AppPortal.Models;
using AppPortal.Services;
using LiteDB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AppPortal.Pages.Admin;

public class IndexModel : PageModel
{
    private readonly IAppService _appService;

    public List<AppEntry> Apps { get; set; } = new();

    public IndexModel(IAppService appService)
    {
        _appService = appService;
    }

    public async Task OnGetAsync()
    {
        Apps = await _appService.GetAllAppsAsync();
    }

    public async Task<IActionResult> OnPostToggleAsync(ObjectId id)
    {
        await _appService.ToggleEnabledAsync(id);
        return RedirectToPage();
    }
}
