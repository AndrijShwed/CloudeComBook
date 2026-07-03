using ClaudeComBook.API.Models;

namespace ClaudeComBook.API.Repositories.Interfaces
{
    public interface IPopulationSnapshotRepository
    {
        Task<IEnumerable<PopulationSnapshot>> GetAllAsync();
        Task<PopulationSnapshot?> GetByIdAsync(int id);
        Task<IEnumerable<PopulationSnapshot>> SearchAsync(string query);
        Task<int> CreateAsync(PopulationSnapshot populationsnapshot);
        Task<bool> UpdateAsync(PopulationSnapshot populationSnapshot);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsForYearAndVillageAsync(int year, string settlementName);
        Task<bool> UpdateByYearAndVillageAsync(int year, string settlementName, int population);
    }
}
