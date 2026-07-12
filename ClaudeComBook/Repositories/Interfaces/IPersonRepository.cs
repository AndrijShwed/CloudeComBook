using ClaudeComBook.API.Models;

namespace ClaudeComBook.API.Repositories.Interfaces;

public interface IPersonRepository
{
    Task<IEnumerable<Person>> GetAllAsync(
    string? lastName = null,
    string? name = null,
    string? surname = null,
    string? sex = null,
    string? status = null,
    string? registr = null,
    int? villageId = null,
    int? streetId = null,
    string? houseNumb = null,
    int? ageFrom = null,
    int? ageTo = null,
    int? statusYear = null,
    string? description = null
    );
    Task<Person?> GetByIdAsync(int id);
    Task<IEnumerable<Person>> GetByVillageStreetIdAsync(int villageStreetId);
    Task<IEnumerable<Person>> SearchAsync(string query);
    Task<int> CreateAsync(Person person);
    Task<bool> UpdateAsync(Person person);
    Task<bool> DeleteAsync(int id);
    Task<Dictionary<string, int>> GetPopulationByVillageAsync();
    Task<bool> ExistsAsync(string lastName, string name, string? surname, DateTime? dateOfBirth);
}
