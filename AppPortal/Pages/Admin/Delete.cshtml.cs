using AppPortal.Models;
using AppPortal.Services;
using LiteDB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AppPortal.Pages.Admin;

public class DeleteModel : PageModel
{
    private readonly IAppService _appService;

    public AppEntry? App { get; set; }

    public DeleteModel(IAppService appService)
    {
        _appService = appService;
    }

    public async Task<IActionResult> OnGetAsync(ObjectId id)
    {
        App = await _appService.GetByIdAsync(id);
        if (App is null)
        {
            return NotFound();
        }
        if (App.IsEnabled)
        {
            TempData["Error"] = "仅已禁用的应用可以删除";
            return RedirectToPage("Index");
        }
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(ObjectId id)
    {
        var app = await _appService.GetByIdAsync(id);
        if (app is null)
        {
            return NotFound();
        }
        if (app.IsEnabled)
        {
            TempData["Error"] = "仅已禁用的应用可以删除";
            return RedirectToPage("Index");
        }
        await _appService.DeleteAsync(id);
        return RedirectToPage("Index");
    }
}
