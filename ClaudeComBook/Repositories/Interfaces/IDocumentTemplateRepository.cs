using ClaudeComBook.API.Models;

namespace ClaudeComBook.API.Repositories.Interfaces;

public interface IDocumentTemplateRepository
{
    Task<IEnumerable<DocumentTemplate>> GetAllAsync();
    Task<DocumentTemplate?> GetByTypeAsync(string type);
    Task<DocumentTemplate?> GetByIdAsync(int id);
    Task<int> CreateAsync(DocumentTemplate template);
    Task<bool> UpdateAsync(DocumentTemplate template);
    Task<bool> DeleteAsync(int id);
}
