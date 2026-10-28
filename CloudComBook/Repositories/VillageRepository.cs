using Dapper;
using CloudComBook.Shared.Models;
using CloudComBook.API.Data;
using CloudComBook.API.Repositories.Interfaces;
using CloudComBook.API.Services;

namespace CloudComBook.API.Repositories;

public class VillageRepository : IVillageRepository
{
    // @communityId = NULL означає «усі громади» (лише супер-адмін без вибраної громади)
    private const string Scope = "(@communityId IS NULL OR community_id = @communityId)";

    private readonly DbConnectionFactory _db;
    private readonly ICurrentUser _user;

    public VillageRepository(DbConnectionFactory db, ICurrentUser user)
    {
        _db = db;
        _user = user;
    }

    public async Task<IEnumerable<Village>> GetAllAsync()
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<Village>(
            $@"SELECT id AS Id, name AS Name, community_id AS CommunityId
               FROM villages
               WHERE {Scope}
               ORDER BY name",
            new { communityId = _user.CommunityId });
    }

    public async Task<Village?> GetByIdAsync(int id)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Village>(
            $@"SELECT id AS Id, name AS Name, community_id AS CommunityId
               FROM villages
               WHERE id = @id AND {Scope}",
            new { id, communityId = _user.CommunityId });
    }

    public async Task<int> CreateAsync(Village village)
    {
        // CommunityId виставляє контролер із токена, значенню з клієнта не довіряємо
        using var conn = _db.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            @"INSERT INTO villages (name, community_id) VALUES (@Name, @CommunityId);
              SELECT LAST_INSERT_ID();", village);
    }

    public async Task<bool> UpdateAsync(Village village)
    {
        // громаду села не змінюємо: оновлюється лише назва
        using var conn = _db.CreateConnection();
        var rows = await conn.ExecuteAsync(
            $"UPDATE villages SET name = @Name WHERE id = @Id AND {Scope}",
            new { village.Name, village.Id, communityId = _user.CommunityId });
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.ExecuteAsync(
            $"DELETE FROM villages WHERE id = @id AND {Scope}",
            new { id, communityId = _user.CommunityId });
        return rows > 0;
    }
}