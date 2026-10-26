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
    private readonly IEnvironmentService _environments;
    private readonly IHostService _hosts;

    public List<DataSource> Sources { get; set; } = new();
    public List<AccessHost> Hosts { get; set; } = new();
    public Dictionary<ObjectId, List<PortainerEnvironment>> EnvironmentsBySource { get; set; } = new();

    [BindProperty]
    public string NewName { get; set; } = string.Empty;

    [BindProperty]
    public DataSourceType NewType { get; set; } = DataSourceType.Portainer;

    [BindProperty]
    public string NewBaseUrl { get; set; } = string.Empty;

    [BindProperty]
    public string NewApiKey { get; set; } = string.Empty;

    [BindProperty]
    public string? EnvId { get; set; }

    [BindProperty]
    public List<string> EnvHostIds { get; set; } = new();

    public string? Message { get; set; }
    public string? Error { get; set; }

    public SourcesModel(IDataSourceService sources, IEnvironmentService environments, IHostService hosts)
    {
        _sources = sources;
        _environments = environments;
        _hosts = hosts;
    }

    public async Task OnGetAsync(string? message, string? error)
    {
        await LoadAsync();
        Message = message;
        Error = error;

        // 进入页面时静默同步所有 Portainer 数据源的 Environment。
        var synced = false;
        foreach (var source in Sources.Where(s => s.Type == DataSourceType.Portainer))
        {
            try
            {
                await _environments.SyncFromPortainerAsync(source);
                synced = true;
            }
            catch (Exception ex)
            {
                Error ??= $"同步「{source.Name}」失败：{ex.Message}";
            }
        }

        if (synced)
        {
            await LoadAsync();
        }
    }

    private async Task LoadAsync()
    {
        Sources = await _sources.GetAllAsync();
        Hosts = await _hosts.GetAllAsync();

        EnvironmentsBySource = new Dictionary<ObjectId, List<PortainerEnvironment>>();
        foreach (var source in Sources)
        {
            EnvironmentsBySource[source.Id] = source.Type == DataSourceType.Portainer
                ? await _environments.GetByDataSourceAsync(source.Id)
                : new List<PortainerEnvironment>();
        }
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
            IsEnabled = true
        };

        var ok = await _sources.TestConnectionAsync(source);
        await _sources.CreateAsync(source);

        if (ok)
        {
            try
            {
                await _environments.SyncFromPortainerAsync(source);
            }
            catch
            {
                // 创建成功但 Environment 同步失败：进入数据源页会自动重试。
            }
        }

        return RedirectToPage(new { message = ok ? "数据源已创建且连接成功" : "数据源已创建，但连接测试失败，请检查 BaseUrl/ApiKey" });
    }

    public async Task<IActionResult> OnPostSyncEnvironmentsAsync(string dataSourceId)
    {
        if (!IdParser.TryParse(dataSourceId, out var oid))
        {
            return RedirectToPage(new { message = "数据源不存在" });
        }

        var source = await _sources.GetByIdAsync(oid);
        if (source is null)
        {
            return RedirectToPage(new { message = "数据源不存在" });
        }

        try
        {
            var environments = await _environments.SyncFromPortainerAsync(source);
            return RedirectToPage(new { message = $"「{source.Name}」已同步 {environments.Count} 个 Environment" });
        }
        catch (Exception ex)
        {
            return RedirectToPage(new { error = $"同步失败：{ex.Message}" });
        }
    }

    public async Task<IActionResult> OnPostUpdateEnvironmentHostsAsync()
    {
        if (!IdParser.TryParse(EnvId, out var oid))
        {
            return RedirectToPage(new { message = "Environment 不存在" });
        }

        await _environments.UpdateHostsAsync(oid, ParseIds(EnvHostIds));
        return RedirectToPage(new { message = "Environment Host 绑定已更新" });
    }

    public async Task<IActionResult> OnPostTestAsync(string id)
    {
        if (!IdParser.TryParse(id, out var oid))
        {
            return RedirectToPage();
        }

        var source = await _sources.GetByIdAsync(oid);
        if (source is null)
        {
            return RedirectToPage(new { message = "数据源不存在" });
        }

        var ok = await _sources.TestConnectionAsync(source);
        return RedirectToPage(new { message = ok ? $"「{source.Name}」连接成功" : $"「{source.Name}」连接失败，请检查 BaseUrl/ApiKey" });
    }

    public async Task<IActionResult> OnPostDeleteAsync(string id)
    {
        if (IdParser.TryParse(id, out var oid))
        {
            await _environments.DeleteByDataSourceAsync(oid);
            await _sources.DeleteAsync(oid);
        }
        return RedirectToPage();
    }

    private static List<ObjectId> ParseIds(List<string>? raw)
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
}
