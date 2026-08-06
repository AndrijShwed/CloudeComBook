using Dapper;
using ClaudeComBook.API.Data;
using ClaudeComBook.Shared.Models;
using ClaudeComBook.API.Repositories.Interfaces;
public class DocumentTemplateRepository : IDocumentTemplateRepository
{
    private readonly DbConnectionFactory _db;
    public DocumentTemplateRepository(DbConnectionFactory db) => _db = db;
    public async Task<IEnumerable<DocumentTemplate>> GetAllAsync()
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<DocumentTemplate>(
            @"SELECT id, name, type, 
          created_at AS CreatedAt, updated_at AS UpdatedAt 
          FROM document_templates ORDER BY name");
    }

    public async Task<DocumentTemplate?> GetByTypeAsync(string type)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<DocumentTemplate>(
            @"SELECT id, name, type,
          created_at AS CreatedAt, updated_at AS UpdatedAt
          FROM document_templates WHERE type = @type", new { type });
    }

    public async Task<int> CreateAsync(DocumentTemplate template)
    {
        using var conn = _db.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            @"INSERT INTO document_templates (name, type)
          VALUES (@Name, @Type);
          SELECT LAST_INSERT_ID();", template);
    }

    public async Task<bool> UpdateAsync(DocumentTemplate template)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.ExecuteAsync(
            @"UPDATE document_templates 
          SET name=@Name, type=@Type, updated_at=NOW()
          WHERE id=@Id", template);
        return rows > 0;
    }

    public Task<DocumentTemplate?> GetByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteAsync(int id)
    {
        throw new NotImplementedException();
    }
}