using Dapper;
using ClaudeComBook.API.Models;
using ClaudeComBook.API.Data;
using ClaudeComBook.API.Repositories.Interfaces;


namespace ClaudeComBook.API.Repositories;

public class VillageRepository : IVillageRepository
{
    private readonly DbConnectionFactory _db;

    public VillageRepository(DbConnectionFactory db) => _db = db;

    public async Task<IEnumerable<Village>> GetAllAsync()
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<Village>(
            "SELECT id, name FROM villages ORDER BY name");
    }

    public async Task<Village?> GetByIdAsync(int id)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Village>(
            "SELECT id, name FROM villages WHERE id = @id", new { id });
    }

    public async Task<int> CreateAsync(Village village)
    {
        using var conn = _db.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            @"INSERT INTO villages (name) VALUES (@Name);
              SELECT LAST_INSERT_ID();", village);
    }

    public async Task<bool> UpdateAsync(Village village)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.ExecuteAsync(
            "UPDATE villages SET name = @Name WHERE id = @Id", village);
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.ExecuteAsync(
            "DELETE FROM villages WHERE id = @id", new { id });
        return rows > 0;
    }

}
