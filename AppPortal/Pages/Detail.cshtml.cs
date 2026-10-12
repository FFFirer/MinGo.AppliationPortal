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
    public AccessHost? ActiveHost { get; set; }
    public List<ModuleView> ModuleViews { get; set; } = new();
    public string? SourceLabel { get; set; }

    public sealed record EndpointView(ResolvedEndpoint Resolved);

    public sealed record ModuleView(
        AppModule Module,
        List<EndpointView> Endpoints,
        bool AllUnreachable,
        int UnreachableCount);

    public async Task<IActionResult> OnGetAsync(string id, string? host)
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
        Hosts = await _hostService.ResolveForAppAsync(app);

        ActiveHost = ResolveActiveHost(host);
        BuildModuleViews();
        SourceLabel = app.SourceId is not null ? $"Source:{app.SourceId}" : "手工录入";
        return Page();
    }

    private AccessHost? ResolveActiveHost(string? hostQuery)
    {
        if (!string.IsNullOrWhiteSpace(hostQuery) && IdParser.TryParse(hostQuery, out var hid))
        {
            var match = Hosts.FirstOrDefault(h => h.Id == hid);
            if (match is not null)
            {
                return match;
            }
        }

        var preferred = App.HostRefs.FirstOrDefault(r => r.IsPreferred);
        if (preferred is not null)
        {
            var byPref = Hosts.FirstOrDefault(h => h.Id == preferred.HostId);
            if (byPref is not null)
            {
                return byPref;
            }
        }

        return Hosts.FirstOrDefault(h => h.IsDefault) ?? Hosts.FirstOrDefault();
    }

    private void BuildModuleViews()
    {
        ModuleViews = App.Modules
            .Where(m => m.IsEnabled)
            .OrderBy(m => m.Order)
            .ThenBy(m => m.Name)
            .Select(m =>
            {
                var endpoints = m.Endpoints
                    .Select(e => new EndpointView(UrlResolver.Resolve(e, ActiveHost)))
                    .ToList();
                var unreachable = endpoints.Count(e => !e.Resolved.IsReachable);
                return new ModuleView(
                    m,
                    endpoints,
                    endpoints.Count > 0 && unreachable == endpoints.Count,
                    unreachable);
            })
            .ToList();
    }
}
