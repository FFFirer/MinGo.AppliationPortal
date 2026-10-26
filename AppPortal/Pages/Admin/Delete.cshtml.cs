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

    public async Task<IActionResult> OnGetAsync(string id)
    {
        ObjectId oid;
        try { oid = new ObjectId(id); }
        catch { return NotFound(); }

        App = await _appService.GetByIdAsync(oid);
        if (App is null)
        {
            return NotFound();
        }
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string id)
    {
        ObjectId oid;
        try { oid = new ObjectId(id); }
        catch { return NotFound(); }

        var app = await _appService.GetByIdAsync(oid);
        if (app is null)
        {
            return NotFound();
        }
        await _appService.DeleteAsync(oid);
        return RedirectToPage("Index");
    }
}
