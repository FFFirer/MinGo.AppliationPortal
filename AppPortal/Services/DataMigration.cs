using AppPortal.Models;
using LiteDB;

namespace AppPortal.Services;

/// <summary>
/// 一次性存量迁移：DataSource → PortainerEnvironment，AppEntry.ExternalId stack Id → compose project name。
/// 迁移标记写入 meta 集合；有数据源迁移失败时保留标记缺失，下次启动重试。
/// </summary>
public static class DataMigration
{
    private const string MarkerId = "portainer-project-env";
    private const string LogPrefix = "[migration]";

    public static async Task RunAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var sp = scope.ServiceProvider;

        var db = sp.GetRequiredService<LiteDatabase>();
        var meta = db.GetCollection<BsonDocument>("meta");
        if (meta.Exists(Query.EQ("_id", MarkerId)))
        {
            return;
        }

        var portainer = sp.GetRequiredService<IPortainerClient>();
        var sources = sp.GetRequiredService<IDataSourceService>();
        var environments = sp.GetRequiredService<IEnvironmentService>();
        var apps = sp.GetRequiredService<IAppService>();

        var all = true;
        var stackCache = new Dictionary<ObjectId, List<PortainerStackInfo>>();
        var legacySources = db.GetCollection<BsonDocument>("datasources");

        foreach (var source in (await sources.GetAllAsync()).Where(s => s.Type == DataSourceType.Portainer))
        {
            try
            {
                var synced = await environments.SyncFromPortainerAsync(source);

                // 旧 DataSource.HostIds 复制到该数据源下所有 Environment（保持原绑定语义）。
                var legacyHostIds = ReadObjectIds(legacySources.FindById(source.Id), "HostIds");
                if (legacyHostIds.Count > 0)
                {
                    foreach (var env in synced.Where(e => e.HostIds.Count == 0))
                    {
                        await environments.UpdateHostsAsync(env.Id, legacyHostIds.ToList());
                    }
                }

                stackCache[source.Id] = await portainer.ListStacksAsync(source.Config);
            }
            catch (Exception ex)
            {
                all = false;
                Console.WriteLine($"{LogPrefix} source '{source.Name}' failed: {ex.Message}");
            }
        }

        foreach (var app in (await apps.GetAllAppsAsync())
                     .Where(a => a.SourceId is not null && a.EnvironmentId is null))
        {
            if (!int.TryParse(app.ExternalId, out var stackId))
            {
                continue;
            }

            var sourceId = app.SourceId!;
            if (!stackCache.TryGetValue(sourceId, out var stacks))
            {
                continue;
            }

            var stack = stacks.FirstOrDefault(s => s.Id == stackId);
            if (stack is null)
            {
                Console.WriteLine($"{LogPrefix} app '{app.Name}': stack {stackId} not found, left unchanged");
                continue;
            }

            var env = (await environments.GetByDataSourceAsync(sourceId))
                .FirstOrDefault(e => e.EndpointId == stack.EndpointId);

            app.ExternalId = stack.Name;
            if (env is not null)
            {
                app.EnvironmentId = env.Id;
            }

            await apps.UpdateAsync(app);
        }

        if (all)
        {
            meta.Upsert(new BsonDocument
            {
                ["_id"] = MarkerId,
                ["appliedAt"] = DateTime.UtcNow
            });
        }
        else
        {
            Console.WriteLine($"{LogPrefix} incomplete, will retry on next start");
        }
    }

    private static List<ObjectId> ReadObjectIds(BsonDocument? doc, string field)
    {
        var result = new List<ObjectId>();
        if (doc is null || !doc.TryGetValue(field, out var value) || !value.IsArray)
        {
            return result;
        }

        foreach (var item in value.AsArray)
        {
            if (item.IsObjectId)
            {
                result.Add(item.AsObjectId);
            }
            else if (item.IsString && TryParseObjectId(item.AsString, out var oid))
            {
                result.Add(oid);
            }
        }

        return result;
    }

    private static bool TryParseObjectId(string? value, out ObjectId oid)
    {
        oid = ObjectId.Empty;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        try
        {
            oid = new ObjectId(value);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
