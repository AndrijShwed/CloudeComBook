using ClaudeComBook.API.Models;

namespace ClaudeComBook.API.Repositories.Interfaces
{
    public interface IStreetRepository
    {
        Task<IEnumerable<Street>> GetAllAsync();
        Task<Street?> GetByIdAsync(int id);
        Task<int> CreateAsync(Street street);
        Task<bool> UpdateAsync(Street street);
        Task<bool> DeleteAsync(int id);
    }
}
