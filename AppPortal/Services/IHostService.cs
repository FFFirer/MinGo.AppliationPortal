using AppPortal.Models;

namespace AppPortal.Services;

public interface IHostService
{
    Task<List<AccessHost>> GetAllAsync();
    Task<AccessHost?> GetByIdAsync(LiteDB.ObjectId id);
    Task CreateAsync(AccessHost host);
    Task UpdateAsync(AccessHost host);
    Task DeleteAsync(LiteDB.ObjectId id);
    Task<List<AccessHost>> ResolveForAppAsync(AppEntry app);
}
