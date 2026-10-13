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

    [BindProperty]
    public string? EditId { get; set; }

    [BindProperty]
    public string EditName { get; set; } = string.Empty;

    [BindProperty]
    public string EditAddresses { get; set; } = string.Empty;

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

        var addresses = ParseAddresses(NewAddresses);
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

    public async Task<IActionResult> OnPostUpdateAsync()
    {
        if (string.IsNullOrWhiteSpace(EditId) || !IdParser.TryParse(EditId, out var oid))
        {
            return RedirectToPage(new { message = "Host 不存在" });
        }

        if (string.IsNullOrWhiteSpace(EditName))
        {
            return RedirectToPage(new { message = "名称必填" });
        }

        var addresses = ParseAddresses(EditAddresses);
        if (addresses.Count == 0)
        {
            return RedirectToPage(new { message = "至少填写一个 IP/地址" });
        }

        var host = await _hostService.GetByIdAsync(oid);
        if (host is null)
        {
            return RedirectToPage(new { message = "Host 不存在" });
        }

        host.Name = EditName.Trim();
        host.Addresses = addresses;
        await _hostService.UpdateAsync(host);
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

    private static List<string> ParseAddresses(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return new List<string>();
        }

        return raw
            .Split(new[] { '\n', '\r', ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s => s.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
