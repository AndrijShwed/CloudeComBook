using CloudeComBook.Shared.Models;

namespace CloudeComBook.API.Repositories.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByLoginAsync(string login);
    Task<User?> GetByIdAsync(int id);
    Task<IEnumerable<User>> GetAllAsync();
    Task<int> CreateAsync(User user);
    Task<bool> UpdateAsync(User user);
    Task<bool> SetActiveAsync(int id, bool isActive);
    Task<bool> DeleteAsync(int id);
}
