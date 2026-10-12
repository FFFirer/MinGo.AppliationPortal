using AppPortal.Models;

namespace AppPortal.Services;

public interface IDataSourceService
{
    Task<List<DataSource>> GetAllAsync();
    Task<DataSource?> GetByIdAsync(LiteDB.ObjectId id);
    Task CreateAsync(DataSource source);
    Task UpdateAsync(DataSource source);
    Task DeleteAsync(LiteDB.ObjectId id);
    Task<bool> TestConnectionAsync(DataSource source);
}
