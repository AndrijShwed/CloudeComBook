using ClaudeComBook.API.Models;

namespace ClaudeComBook.API.Repositories.Interfaces
{
    public interface IHouseRepository
    {
        Task<IEnumerable<House>> GetAllAsync();
        Task<House?> GetByIdAsync(int id);
        Task<IEnumerable<House>> GetByVillageStreetIdAsync(int villageStreetId);
        Task<IEnumerable<House>> SearchAsync(string query);
        Task<int> CreateAsync(House house);
        Task<bool> UpdateAsync(House house);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int villageStreetId, string numbOfHouse);
    }
}
