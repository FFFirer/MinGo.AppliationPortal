using AppPortal.Models;
using LiteDB;

namespace AppPortal.Services;

public class DataSourceService : IDataSourceService
{
    private readonly ILiteCollection<DataSource> _sources;
    private readonly IPortainerClient _portainer;

    public DataSourceService(LiteDatabase db, IPortainerClient portainer)
    {
        _sources = db.GetCollection<DataSource>("datasources");
        _portainer = portainer;
    }

    public Task<List<DataSource>> GetAllAsync()
        => Task.FromResult(_sources.FindAll().OrderBy(s => s.Name).ToList());

    public Task<DataSource?> GetByIdAsync(ObjectId id)
        => Task.FromResult(_sources.FindById(id));

    public Task CreateAsync(DataSource source)
    {
        _sources.Insert(source);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(DataSource source)
    {
        _sources.Update(source);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(ObjectId id)
    {
        _sources.Delete(id);
        return Task.CompletedTask;
    }

    public async Task<bool> TestConnectionAsync(DataSource source)
    {
        try
        {
            if (source.Type == DataSourceType.Portainer)
            {
                return await _portainer.TestAsync(source.Config);
            }

            return source.Type == DataSourceType.Manual || source.Type == DataSourceType.ComposeFile;
        }
        catch
        {
            return false;
        }
    }
}
