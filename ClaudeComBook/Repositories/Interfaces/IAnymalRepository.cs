using ClaudeComBook.API.Models;

namespace ClaudeComBook.API.Repositories.Interfaces
{
    public interface IAnymalRepository
    {
        Task<IEnumerable<Anymal>> GetAllAsync();
        Task<Anymal?> GetByIdAsync(int id);
        Task<IEnumerable<Anymal>> SearchAsync(string query);
        Task<int> CreateAsync(Anymal anymal);
        Task<bool> UpdateAsync(Anymal anymal);
        Task<bool> DeleteAsync(int id);
    }
}
