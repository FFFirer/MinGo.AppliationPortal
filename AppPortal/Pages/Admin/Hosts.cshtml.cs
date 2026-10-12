using AppPortal.Models;
using AppPortal.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AppPortal.Pages.Admin;

public class HostsModel : PageModel
{
    private readonly IHostService _hostService;

    public List<AccessHost> Hosts { get; set; } = new();

    [BindProperty]
    public AccessHost NewHost { get; set; } = new();

    public string? Message { get; set; }

    public HostsModel(IHostService hostService)
    {
        _hostService = hostService;
    }

    public async Task OnGetAsync(string? message)
    {
        Hosts = await _hostService.GetAllAsync();
        Message = message;
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (string.IsNullOrWhiteSpace(NewHost.Name) || string.IsNullOrWhiteSpace(NewHost.Address))
        {
            return RedirectToPage(new { message = "名称与地址必填" });
        }

        NewHost.SchemeDefault = string.IsNullOrWhiteSpace(NewHost.SchemeDefault) ? "http" : NewHost.SchemeDefault;
        await _hostService.CreateAsync(NewHost);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(string id)
    {
        if (IdParser.TryParse(id, out var oid))
        {
            await _hostService.DeleteAsync(oid);
        }
        return RedirectToPage();
    }
}
