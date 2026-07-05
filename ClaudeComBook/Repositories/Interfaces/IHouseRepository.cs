using ClaudeComBook.API.Models;

namespace ClaudeComBook.API.Repositories.Interfaces
{
    public interface IHouseRepository
    {
        Task<IEnumerable<House>> GetAllAsync();
        Task<House?> GetByIdAsync(int id);
        Task<IEnumerable<House>> GetByVillageStreetIdAsync(int villageStreetId);
        Task<IEnumerable<House>> SearchAsync(
                        int? villageId = null,
                        int? streetId = null,
                        string? houseNumber = null,
                        string? lastName = null,
                        string? name = null,
                        string? surname = null);
        Task<int> CreateAsync(House house);
        Task<bool> UpdateAsync(House house);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int villageStreetId, string numbOfHouse);
        Task<IEnumerable<(string Village, decimal TotalArea, decimal LivingArea)>> GetAreaByVillageAsync();
    }
}
