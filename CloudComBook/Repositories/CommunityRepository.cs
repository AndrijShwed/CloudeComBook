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
            @"SELECT c.id AS Id, c.name AS Name, c.is_active AS IsActive,
                     (SELECT COUNT(*) FROM users u WHERE u.community_id = c.id) AS UsersCount,
                     (SELECT COUNT(*) FROM villages v WHERE v.community_id = c.id) AS VillagesCount
              FROM communities c
              ORDER BY c.name");
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

    public async Task<bool> DeleteAsync(int id)
    {
        using var conn = _db.CreateConnection();
        return await conn.ExecuteAsync("DELETE FROM communities WHERE id = @id", new { id }) > 0;
    }

    public async Task<CommunityUsage> GetUsageAsync(int id)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstAsync<CommunityUsage>(
            @"SELECT
                (SELECT COUNT(*) FROM users    WHERE community_id = @id) AS Users,
                (SELECT COUNT(*) FROM villages WHERE community_id = @id) AS Villages,
                (SELECT COUNT(*) FROM streets  WHERE community_id = @id) AS Streets,
                (SELECT COUNT(*) FROM plot     WHERE community_id = @id) AS Plots,
                (SELECT COUNT(*) FROM anymals  WHERE community_id = @id) AS Anymals",
            new { id });
    }
}