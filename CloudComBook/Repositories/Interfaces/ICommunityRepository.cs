// Repositories/Interfaces/ICommunityRepository.cs
using CloudComBook.Shared.Models;

public interface ICommunityRepository
{
    Task<IEnumerable<Community>> GetAllAsync();
    Task<Community?> GetByIdAsync(int id);
    Task<int> CreateAsync(Community community);
    Task<bool> UpdateAsync(Community community);
}
