using Dapper;
using ClaudeComBook.API.Data;
using ClaudeComBook.API.Models;
using ClaudeComBook.API.Repositories.Interfaces;

namespace ClaudeComBook.API.Repositories;
public class StreetRepository : IStreetRepository
{
    private readonly DbConnectionFactory _db;

    public StreetRepository(DbConnectionFactory db) => _db = db;
    public async Task<IEnumerable<Street>> GetAllAsync()
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<Street>(
            "SELECT id, name FROM streets ORDER BY name");
    }
    public async Task<Street?> GetByIdAsync(int id)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Street>(
            "SELECT id, name FROM streets WHERE id = @id", new { id });
    }

    public async Task<int> CreateAsync(Street street)
    {
        using var conn = _db.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            @"INSERT INTO streets (name) VALUES (@Name);
              SELECT LAST_INSERT_ID();", street);
    }

    public async Task<bool> UpdateAsync(Street street)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.ExecuteAsync(
            "UPDATE streets SET name = @Name WHERE id = @Id", street);
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.ExecuteAsync(
            "DELETE FROM streets WHERE id = @id", new { id });
        return rows > 0;
    }
}

