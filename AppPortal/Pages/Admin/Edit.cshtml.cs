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

    public async Task<IActionResult> OnGetAsync(string id)
    {
        ObjectId oid;
        try { oid = new ObjectId(id); }
        catch { return NotFound(); }

        var app = await _appService.GetByIdAsync(oid);
        if (app is null)
        {
            return NotFound();
        }
        App = app;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string id)
    {
        ObjectId oid;
        try { oid = new ObjectId(id); }
        catch { return NotFound(); }

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
        App.Id = oid;
        await _appService.UpdateAsync(App);
        return RedirectToPage("Index");
    }
}
