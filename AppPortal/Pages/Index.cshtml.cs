using AppPortal.Models;
using AppPortal.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AppPortal.Pages;

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
        Apps = await _appService.GetEnabledAppsAsync();
    }
}
