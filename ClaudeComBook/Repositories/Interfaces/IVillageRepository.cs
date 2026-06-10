using ClaudeComBook.API.Models;

namespace ClaudeComBook.API.Repositories.Interfaces
{
    public interface IVillageRepository
    {
        Task<IEnumerable<Village>> GetAllAsync();
        Task<Village?> GetByIdAsync(int id);
        Task<int> CreateAsync(Village village);
        Task<bool> UpdateAsync(Village village);
        Task<bool> DeleteAsync(int id);
    }
}
