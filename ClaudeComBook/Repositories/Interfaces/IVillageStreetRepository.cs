using ClaudeComBook.API.Models;

namespace ClaudeComBook.API.Repositories.Interfaces
{
    public interface IVillageStreetRepository
    {
        Task<IEnumerable<VillageStreet>> GetAllAsync();
        Task<VillageStreet?> GetByIdAsync(int id);
        Task<int> CreateAsync(VillageStreet villageStreet);
        Task<bool> UpdateAsync(VillageStreet villageStreet);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<VillageStreet>> GetByVillageIdAsync(int villageId);
        Task<IEnumerable<VillageStreet>> GetByStreetIdAsync(int streetId);
        Task<bool> UpdateFileAsync(int id, byte[] fileData);
    }
}
