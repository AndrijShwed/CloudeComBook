using CloudeComBook.Shared.Models;

namespace CloudeComBook.API.Repositories.Interfaces
{
    public interface IPlotRepository
    {
        Task<IEnumerable<Plot>> GetAllAsync();
        Task<Plot?> GetByIdAsync(int id);
        Task<IEnumerable<Plot>> SearchAsync(string query);
        Task<int> CreateAsync(Plot plot);
        Task<bool> UpdateAsync(Plot plot);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<Plot>> SearchAsync(
            string? fullName = null,
            string? village = null,
            string? street = null,
            string? houseNumb = null,
            string? fieldNumber = null,
            string? plotType = null,
            string? plotNumber = null,
            string? tenant = null,
            string? cadastr = null);

        Task<bool> ExistsAsync(
            string? cadastr, 
            string? village, 
            string? street, 
            string? houseNumb, 
            string? plotType, 
            int? excludeId = null);
    }
}
