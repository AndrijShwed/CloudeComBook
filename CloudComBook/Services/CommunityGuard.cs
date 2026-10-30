using CloudComBook.API.Data;
using Dapper;

namespace CloudComBook.API.Services;

public interface ICommunityGuard
{
    Task<bool> VillageStreetAllowedAsync(int? villageStreetId);
    Task<bool> VillageAllowedAsync(int villageId);
    Task<bool> StreetAllowedAsync(int streetId);
    Task<bool> SameCommunityAsync(int villageId, int streetId);
    Task<bool> VillageNameInCommunityAsync(string? villageName, int communityId);
}

public class CommunityGuard : ICommunityGuard
{
    private readonly DbConnectionFactory _db;
    private readonly ICurrentUser _user;

    public CommunityGuard(DbConnectionFactory db, ICurrentUser user)
    {
        _db = db;
        _user = user;
    }

    // @c = NULL означає «усі громади» (лише супер-адмін без вибраної громади)

    public async Task<bool> VillageStreetAllowedAsync(int? villageStreetId)
    {
        if (villageStreetId == null) return false;

        using var conn = _db.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            @"SELECT COUNT(*) FROM villagestreet vs
              JOIN villages v ON v.id = vs.villageId
              WHERE vs.id = @id AND (@c IS NULL OR v.community_id = @c)",
            new { id = villageStreetId, c = _user.CommunityId }) > 0;
    }

    public async Task<bool> VillageAllowedAsync(int villageId)
    {
        using var conn = _db.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM villages WHERE id = @id AND (@c IS NULL OR community_id = @c)",
            new { id = villageId, c = _user.CommunityId }) > 0;
    }

    public async Task<bool> StreetAllowedAsync(int streetId)
    {
        using var conn = _db.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM streets WHERE id = @id AND (@c IS NULL OR community_id = @c)",
            new { id = streetId, c = _user.CommunityId }) > 0;
    }
    public async Task<bool> SameCommunityAsync(int villageId, int streetId)
    {
        using var conn = _db.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            @"SELECT COUNT(*) FROM villages v
          JOIN streets s ON s.community_id = v.community_id
          WHERE v.id = @v AND s.id = @s",
            new { v = villageId, s = streetId }) > 0;
    }
    public async Task<bool> VillageNameInCommunityAsync(string? villageName, int communityId)
    {
        if (string.IsNullOrWhiteSpace(villageName)) return false;

        using var conn = _db.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            @"SELECT COUNT(*) FROM villages
          WHERE community_id = @c AND LOWER(TRIM(name)) = LOWER(TRIM(@n))",
            new { c = communityId, n = villageName }) > 0;
    }
}