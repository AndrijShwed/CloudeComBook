using Dapper;
using ClaudeComBook.API.Data;
using ClaudeComBook.API.Models;
using ClaudeComBook.API.Repositories.Interfaces;

namespace ClaudeComBook.API.Repositories;

public class DocumentTemplateRepository : IDocumentTemplateRepository
{
    private readonly DbConnectionFactory _db;
    public DocumentTemplateRepository(DbConnectionFactory db) => _db = db;

    public async Task<IEnumerable<DocumentTemplate>> GetAllAsync()
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<DocumentTemplate>(
            "SELECT id, name, type, created_at AS CreatedAt, updated_at AS UpdatedAt FROM document_templates ORDER BY name");
    }

    public async Task<DocumentTemplate?> GetByTypeAsync(string type)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<DocumentTemplate>(
            "SELECT * FROM document_templates WHERE type = @type", new { type });
    }

    public async Task<DocumentTemplate?> GetByIdAsync(int id)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<DocumentTemplate>(
            "SELECT * FROM document_templates WHERE id = @id", new { id });
    }

    public async Task<int> CreateAsync(DocumentTemplate template)
    {
        using var conn = _db.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            @"INSERT INTO document_templates (name, type, template)
              VALUES (@Name, @Type, @Template);
              SELECT LAST_INSERT_ID();", template);
    }

    public async Task<bool> UpdateAsync(DocumentTemplate template)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.ExecuteAsync(
            @"UPDATE document_templates SET name=@Name, type=@Type, template=@Template
              WHERE id=@Id", template);
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.ExecuteAsync(
            "DELETE FROM document_templates WHERE id=@id", new { id });
        return rows > 0;
    }
}
