using Dapper;
using CloudComBook.API.Data;
using CloudComBook.API.Services;
using CloudComBook.Shared.Models;
using CloudComBook.API.Repositories.Interfaces;

namespace CloudComBook.API.Repositories;

public class StreetRepository : IStreetRepository
{
    // @communityId = NULL означає «усі громади» (лише супер-адмін без вибраної громади)
    private const string Scope = "(@communityId IS NULL OR community_id = @communityId)";

    private readonly DbConnectionFactory _db;
    private readonly ICurrentUser _user;

    public StreetRepository(DbConnectionFactory db, ICurrentUser user)
    {
        _db = db;
        _user = user;
    }

    public async Task<IEnumerable<Street>> GetAllAsync()
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<Street>(
            $@"SELECT id AS Id, name AS Name, community_id AS CommunityId
               FROM streets
               WHERE {Scope}
               ORDER BY name",
            new { communityId = _user.CommunityId });
    }

    public async Task<Street?> GetByIdAsync(int id)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Street>(
            $@"SELECT id AS Id, name AS Name, community_id AS CommunityId
               FROM streets
               WHERE id = @id AND {Scope}",
            new { id, communityId = _user.CommunityId });
    }

    public async Task<int> CreateAsync(Street street)
    {
        // CommunityId виставляє контролер із токена, значенню з клієнта не довіряємо
        using var conn = _db.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            @"INSERT INTO streets (name, community_id) VALUES (@Name, @CommunityId);
              SELECT LAST_INSERT_ID();", street);
    }

    public async Task<bool> UpdateAsync(Street street)
    {
        // громаду вулиці не змінюємо: оновлюється лише назва
        using var conn = _db.CreateConnection();
        var rows = await conn.ExecuteAsync(
            $"UPDATE streets SET name = @Name WHERE id = @Id AND {Scope}",
            new { street.Name, street.Id, communityId = _user.CommunityId });
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.ExecuteAsync(
            $"DELETE FROM streets WHERE id = @id AND {Scope}",
            new { id, communityId = _user.CommunityId });
        return rows > 0;
    }
}