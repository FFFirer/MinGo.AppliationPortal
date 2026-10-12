using AppPortal.Models;
using AppPortal.Services;
using LiteDB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AppPortal.Pages;

public class DetailModel : PageModel
{
    private readonly IAppService _appService;

    public DetailModel(IAppService appService)
    {
        _appService = appService;
    }

    public AppEntry App { get; set; } = new();
    public List<ModuleView> ModuleViews { get; set; } = new();
    public string? SourceLabel { get; set; }

    public sealed record AddressView(ModuleAddress Address, string Url);

    public sealed record ModuleView(AppModule Module, List<AddressView> Addresses);

    public async Task<IActionResult> OnGetAsync(string id)
    {
        if (!IdParser.TryParse(id, out var oid))
        {
            return NotFound();
        }

        var app = await _appService.GetByIdAsync(oid);
        if (app is null)
        {
            return NotFound();
        }

        App = app;
        ModuleViews = app.Modules
            .Where(m => m.IsEnabled)
            .OrderBy(m => m.Order)
            .ThenBy(m => m.Name)
            .Select(m => new ModuleView(
                m,
                m.Addresses
                    .Select(a =>
                    {
                        var r = UrlResolver.Resolve(a);
                        return new AddressView(a, r.Url);
                    })
                    .ToList()))
            .ToList();

        SourceLabel = app.SourceId is not null ? $"Source:{app.SourceId}" : "手工录入";
        return Page();
    }
}
