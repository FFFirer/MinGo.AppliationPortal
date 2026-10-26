using AppPortal.Models;
using LiteDB;

namespace AppPortal.Services;

public interface IEnvironmentService
{
    Task<List<PortainerEnvironment>> GetAllAsync();
    Task<List<PortainerEnvironment>> GetByDataSourceAsync(ObjectId dataSourceId);
    Task<PortainerEnvironment?> GetByIdAsync(ObjectId id);
    Task CreateAsync(PortainerEnvironment environment);
    Task UpdateAsync(PortainerEnvironment environment);
    Task UpdateHostsAsync(ObjectId id, List<ObjectId> hostIds);
    Task DeleteAsync(ObjectId id);
    Task DeleteByDataSourceAsync(ObjectId dataSourceId);

    /// <summary>从 Portainer /api/endpoints 拉取并 upsert 该数据源下的 Environment。</summary>
    Task<List<PortainerEnvironment>> SyncFromPortainerAsync(DataSource source);
}
