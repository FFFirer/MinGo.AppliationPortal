using AppPortal.Models;
using LiteDB;

namespace AppPortal.Services;

public interface IAppService
{
    Task<List<AppEntry>> GetAllAppsAsync();
    Task<List<AppEntry>> GetEnabledAppsAsync();
    Task<AppEntry?> GetByIdAsync(ObjectId id);
    Task CreateAsync(AppEntry app);
    Task UpdateAsync(AppEntry app);
    Task DeleteAsync(ObjectId id);
    Task ToggleEnabledAsync(ObjectId id);
}
