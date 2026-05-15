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
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(ObjectId id)
    {
        await _appService.DeleteAsync(id);
        return RedirectToPage("Index");
    }
}
