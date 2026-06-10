using ClaudeComBook.API.Models;

namespace ClaudeComBook.API.Repositories.Interfaces;

public interface IPersonRepository
{
    Task<IEnumerable<Person>> GetAllAsync();
    Task<Person?> GetByIdAsync(int id);
    Task<IEnumerable<Person>> GetByVillageStreetIdAsync(int villageStreetId);
    Task<IEnumerable<Person>> SearchAsync(string query);
    Task<int> CreateAsync(Person person);
    Task<bool> UpdateAsync(Person person);
    Task<bool> DeleteAsync(int id);
}
