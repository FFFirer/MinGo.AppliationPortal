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

    public sealed record ModuleView(AppModule Module, List<string> Urls);

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
            .Select(m => new ModuleView(m, m.Addresses.Where(u => !string.IsNullOrWhiteSpace(u)).ToList()))
            .ToList();

        SourceLabel = app.SourceId is not null ? $"Source:{app.SourceId}" : "手工录入";
        return Page();
    }
}
