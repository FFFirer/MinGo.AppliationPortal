using System.Text.Json;
using AppPortal.Models;
using AppPortal.Services;
using LiteDB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AppPortal.Pages.Admin;

public class SourcesModel : PageModel
{
    private readonly IDataSourceService _sources;
    private readonly IHostService _hosts;

    public List<DataSource> Sources { get; set; } = new();
    public List<AccessHost> Hosts { get; set; } = new();

    [BindProperty]
    public string NewName { get; set; } = string.Empty;

    [BindProperty]
    public DataSourceType NewType { get; set; } = DataSourceType.Portainer;

    [BindProperty]
    public string NewBaseUrl { get; set; } = string.Empty;

    [BindProperty]
    public string NewApiKey { get; set; } = string.Empty;

    [BindProperty]
    public List<string> NewHostIds { get; set; } = new();

    [BindProperty]
    public string? EditId { get; set; }

    [BindProperty]
    public List<string> EditHostIds { get; set; } = new();

    public string? Message { get; set; }

    public SourcesModel(IDataSourceService sources, IHostService hosts)
    {
        _sources = sources;
        _hosts = hosts;
    }

    public async Task OnGetAsync(string? message)
    {
        Sources = await _sources.GetAllAsync();
        Hosts = await _hosts.GetAllAsync();
        Message = message;
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (string.IsNullOrWhiteSpace(NewName))
        {
            return RedirectToPage(new { message = "名称必填" });
        }

        var config = System.Text.Json.JsonSerializer.Serialize(new PortainerConfig
        {
            BaseUrl = NewBaseUrl.Trim(),
            ApiKey = NewApiKey.Trim()
        });

        var source = new DataSource
        {
            Name = NewName.Trim(),
            Type = NewType,
            Config = config,
            IsEnabled = true,
            HostIds = ParseHostIds(NewHostIds)
        };

        var ok = await _sources.TestConnectionAsync(source);
        source.LastStatus = ok ? "connected" : "unreachable";
        await _sources.CreateAsync(source);
        return RedirectToPage(new { message = ok ? "数据源已创建且连接成功" : "数据源已创建，但连接测试失败，请检查 BaseUrl/ApiKey" });
    }

    public async Task<IActionResult> OnPostUpdateHostsAsync()
    {
        if (!IdParser.TryParse(EditId, out var oid))
        {
            return RedirectToPage(new { message = "数据源不存在" });
        }

        var source = await _sources.GetByIdAsync(oid);
        if (source is null)
        {
            return RedirectToPage(new { message = "数据源不存在" });
        }

        source.HostIds = ParseHostIds(EditHostIds);
        await _sources.UpdateAsync(source);
        return RedirectToPage(new { message = "Host 绑定已更新" });
    }

    private static List<ObjectId> ParseHostIds(List<string>? raw)
    {
        var ids = new List<ObjectId>();
        if (raw is null)
        {
            return ids;
        }

        foreach (var s in raw)
        {
            if (IdParser.TryParse(s, out var oid) && !ids.Contains(oid))
            {
                ids.Add(oid);
            }
        }

        return ids;
    }

    public async Task<IActionResult> OnPostTestAsync(string id)
    {
        if (IdParser.TryParse(id, out var oid))
        {
            var source = await _sources.GetByIdAsync(oid);
            if (source is not null)
            {
                var ok = await _sources.TestConnectionAsync(source);
                source.LastStatus = ok ? "connected" : "unreachable";
                source.LastSyncAt = DateTime.UtcNow;
                await _sources.UpdateAsync(source);
            }
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(string id)
    {
        if (IdParser.TryParse(id, out var oid))
        {
            await _sources.DeleteAsync(oid);
        }
        return RedirectToPage();
    }
}
