using AppPortal.Models;
using AppPortal.Services;
using LiteDB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AppPortal.Pages;

public class DetailModel : PageModel
{
    private readonly IAppService _appService;
    private readonly IHostService _hostService;

    public DetailModel(IAppService appService, IHostService hostService)
    {
        _appService = appService;
        _hostService = hostService;
    }

    public AppEntry App { get; set; } = new();
    public List<AccessHost> Hosts { get; set; } = new();
    public List<ModuleView> ModuleViews { get; set; } = new();
    public string? SourceLabel { get; set; }

    public sealed record PortView(ResolvedPort Resolved, string Label);

    public sealed record ModuleView(
        AppModule Module,
        AccessHost? Host,
        string SchemeText,
        string PathText,
        List<PortView> Ports,
        bool AllUnreachable);

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
        Hosts = await _hostService.GetAllAsync();
        BuildModuleViews();
        SourceLabel = app.SourceId is not null ? $"Source:{app.SourceId}" : "手工录入";
        return Page();
    }

    private void BuildModuleViews()
    {
        var hostMap = Hosts.ToDictionary(h => h.Id, h => h);

        ModuleViews = App.Modules
            .Where(m => m.IsEnabled)
            .OrderBy(m => m.Order)
            .ThenBy(m => m.Name)
            .Select(m =>
            {
                AccessHost? host = null;
                if (m.HostId is { } hid)
                {
                    hostMap.TryGetValue(hid, out host);
                }
                else if (Hosts.Count > 0)
                {
                    host = Hosts.FirstOrDefault(h => h.IsDefault) ?? Hosts[0];
                }

                var ports = m.Ports
                    .Select(p =>
                    {
                        var resolved = UrlResolver.Resolve(p, m, host);
                        return new PortView(resolved, p.Port > 0 ? $":{p.Port}" : "入口");
                    })
                    .ToList();

                var allUnreachable = ports.Count > 0 && ports.All(p => !p.Resolved.IsReachable);
                var pathText = string.IsNullOrWhiteSpace(m.Path) ? "/" : m.Path.Trim();
                return new ModuleView(
                    m,
                    host,
                    UrlResolver.NormalizeScheme(m.Scheme),
                    pathText,
                    ports,
                    allUnreachable);
            })
            .ToList();
    }
}
