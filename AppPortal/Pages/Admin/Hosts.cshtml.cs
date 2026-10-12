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
    public string NewName { get; set; } = string.Empty;

    /// <summary>One address per line, or comma-separated.</summary>
    [BindProperty]
    public string NewAddresses { get; set; } = string.Empty;

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
        if (string.IsNullOrWhiteSpace(NewName))
        {
            return RedirectToPage(new { message = "名称必填" });
        }

        var addresses = NewAddresses
            .Split(new[] { '\n', '\r', ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s => s.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (addresses.Count == 0)
        {
            return RedirectToPage(new { message = "至少填写一个 IP/地址" });
        }

        await _hostService.CreateAsync(new AccessHost
        {
            Name = NewName.Trim(),
            Addresses = addresses,
            IsDefault = false
        });
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
