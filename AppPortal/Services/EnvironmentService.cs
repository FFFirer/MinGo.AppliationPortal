using AppPortal.Models;
using LiteDB;

namespace AppPortal.Services;

public class EnvironmentService : IEnvironmentService
{
    private readonly ILiteCollection<PortainerEnvironment> _environments;
    private readonly IPortainerClient _portainer;

    public EnvironmentService(LiteDatabase db, IPortainerClient portainer)
    {
        _environments = db.GetCollection<PortainerEnvironment>("environments");
        _environments.EnsureIndex(x => x.DataSourceId);
        _portainer = portainer;
    }

    public Task<List<PortainerEnvironment>> GetAllAsync()
        => Task.FromResult(_environments.FindAll()
            .OrderBy(e => e.Name)
            .ThenBy(e => e.EndpointId)
            .ToList());

    public Task<List<PortainerEnvironment>> GetByDataSourceAsync(ObjectId dataSourceId)
        => Task.FromResult(_environments.Find(e => e.DataSourceId == dataSourceId)
            .OrderBy(e => e.EndpointId)
            .ToList());

    public Task<PortainerEnvironment?> GetByIdAsync(ObjectId id)
        => Task.FromResult<PortainerEnvironment?>(_environments.FindById(id));

    public Task CreateAsync(PortainerEnvironment environment)
    {
        _environments.Insert(environment);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(PortainerEnvironment environment)
    {
        _environments.Update(environment);
        return Task.CompletedTask;
    }

    public Task UpdateHostsAsync(ObjectId id, List<ObjectId> hostIds)
    {
        var env = _environments.FindById(id);
        if (env is not null)
        {
            env.HostIds = hostIds;
            _environments.Update(env);
        }
        return Task.CompletedTask;
    }

    public Task DeleteAsync(ObjectId id)
    {
        _environments.Delete(id);
        return Task.CompletedTask;
    }

    public Task DeleteByDataSourceAsync(ObjectId dataSourceId)
    {
        _environments.DeleteMany(e => e.DataSourceId == dataSourceId);
        return Task.CompletedTask;
    }

    public async Task<List<PortainerEnvironment>> SyncFromPortainerAsync(DataSource source)
    {
        if (source.Type != DataSourceType.Portainer)
        {
            return await GetByDataSourceAsync(source.Id);
        }

        var endpoints = await _portainer.ListEndpointsAsync(source.Config);
        var existing = await GetByDataSourceAsync(source.Id);
        var now = DateTime.UtcNow;

        foreach (var endpoint in endpoints)
        {
            var env = existing.FirstOrDefault(e => e.EndpointId == endpoint.Id);
            if (env is null)
            {
                _environments.Insert(new PortainerEnvironment
                {
                    DataSourceId = source.Id,
                    EndpointId = endpoint.Id,
                    Name = endpoint.Name,
                    Type = endpoint.Type,
                    LastSyncAt = now
                });
                continue;
            }

            env.Name = endpoint.Name;
            env.Type = endpoint.Type;
            env.LastSyncAt = now;
            _environments.Update(env);
        }

        return await GetByDataSourceAsync(source.Id);
    }
}
