using CloudeComBook.Shared.Models;
using CloudeComBook.Shared.DTOs;

namespace CloudeComBook.API.Repositories.Interfaces
{
    public interface IAnymalRepository
    {
        Task<IEnumerable<Anymal>> GetAllAsync();
        Task<Anymal?> GetByIdAsync(int id);
        Task<IEnumerable<Anymal>> SearchAsync(string query);
        Task<int> CreateAsync(Anymal anymal);
        Task<bool> UpdateAsync(Anymal anymal);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(string lastName, string name, string? surname, string village);
        Task<IEnumerable<Anymal>> SearchAsync(
                     string? lastName = null,
                     string? name = null,
                     string? surname = null,
                     string? village = null,
                     bool hasCovs = false,
                     bool hasHorses = false,
                     bool hasPigs = false,
                     bool hasSheeps = false,
                     bool hasGoats = false,
                     bool hasBirds = false,
                     bool hasRabbits = false,
                     bool hasBeeses = false);
        Task<IEnumerable<AnymalVillageStatisticsDto>> GetStatisticsByVillageAsync();
    }
}
