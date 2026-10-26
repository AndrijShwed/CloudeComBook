// Repositories/CommunityRepository.cs
using CloudComBook.API.Data;
using CloudComBook.API.Repositories.Interfaces;
using CloudComBook.Shared.Models;
using Dapper;

namespace CloudComBook.API.Repositories;

public class CommunityRepository : ICommunityRepository
{
    private readonly DbConnectionFactory _db;
    public CommunityRepository(DbConnectionFactory db) => _db = db;

    public async Task<IEnumerable<Community>> GetAllAsync()
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<Community>(
            "SELECT id AS Id, name AS Name, is_active AS IsActive FROM communities ORDER BY name");
    }

    public async Task<Community?> GetByIdAsync(int id)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Community>(
            "SELECT id AS Id, name AS Name, is_active AS IsActive FROM communities WHERE id = @id",
            new { id });
    }

    public async Task<int> CreateAsync(Community c)
    {
        using var conn = _db.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            @"INSERT INTO communities (name, is_active) VALUES (@Name, @IsActive);
              SELECT LAST_INSERT_ID();", c);
    }

    public async Task<bool> UpdateAsync(Community c)
    {
        using var conn = _db.CreateConnection();
        return await conn.ExecuteAsync(
            "UPDATE communities SET name = @Name, is_active = @IsActive WHERE id = @Id", c) > 0;
    }
}
