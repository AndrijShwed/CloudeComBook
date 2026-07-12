using ClaudeComBook.API.Models;

namespace ClaudeComBook.API.Repositories.Interfaces
{
    public interface IEnterpriseRepository
    {
        Task<IEnumerable<Enterprise>> GetAllAsync();
        Task<Enterprise?> GetByIdAsync(int id);
        Task<IEnumerable<Enterprise>> GetByVillageStreetIdAsync(int villageStreetId);
        Task<IEnumerable<Enterprise>> SearchAsync(string query);
        Task<int> CreateAsync(Enterprise house);
        Task<bool> UpdateAsync(Enterprise house);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<Enterprise>> SearchAsync(
            string? name = null,
            string? owner = null,
            int? villageId = null,
            int? streetId = null,
            string? houseNumber = null);
        Task<bool> ExistsByNameAsync(string name);
    }
}
