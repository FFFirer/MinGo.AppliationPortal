using AppPortal.Models;
using AppPortal.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AppPortal.Pages.Admin;

public class CreateModel : PageModel
{
    private readonly IAppService _appService;

    [BindProperty]
    public AppEntry App { get; set; } = new()
    {
        Urls = new List<AppUrl> { new() }
    };

    public CreateModel(IAppService appService)
    {
        _appService = appService;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(App.Name))
        {
            ModelState.AddModelError("App.Name", "应用名称不能为空");
        }

        // Validate at least one URL is present and non-empty
        var validUrls = App.Urls?.Where(u => !string.IsNullOrWhiteSpace(u.Url)).ToList() ?? new List<AppUrl>();
        if (validUrls.Count == 0)
        {
            ModelState.AddModelError("App.Urls", "至少需要一个访问地址");
        }

        if (!ModelState.IsValid)
        {
            App.Urls = validUrls.Count > 0 ? validUrls : new List<AppUrl> { new() };
            return Page();
        }

        App.Urls = validUrls;
        await _appService.CreateAsync(App);
        return RedirectToPage("Index");
    }
}
