using ClaudeComBook.API.Models;

namespace ClaudeComBook.API.Repositories.Interfaces
{
    public interface IPlotRepository
    {
        Task<IEnumerable<Plot>> GetAllAsync();
        Task<Plot?> GetByIdAsync(int id);
        Task<IEnumerable<Plot>> SearchAsync(string query);
        Task<int> CreateAsync(Plot plot);
        Task<bool> UpdateAsync(Plot plot);
        Task<bool> DeleteAsync(int id);
    }
}
