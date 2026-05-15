using AppPortal.Models;
using AppPortal.Services;
using LiteDB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AppPortal.Pages.Admin;

public class EditModel : PageModel
{
    private readonly IAppService _appService;

    [BindProperty]
    public AppEntry App { get; set; } = new();

    public EditModel(IAppService appService)
    {
        _appService = appService;
    }

    public async Task<IActionResult> OnGetAsync(ObjectId id)
    {
        var app = await _appService.GetByIdAsync(id);
        if (app is null)
        {
            return NotFound();
        }
        App = app;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(ObjectId id)
    {
        if (string.IsNullOrWhiteSpace(App.Name))
        {
            ModelState.AddModelError("App.Name", "应用名称不能为空");
        }

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
        App.Id = id;
        await _appService.UpdateAsync(App);
        return RedirectToPage("Index");
    }
}
