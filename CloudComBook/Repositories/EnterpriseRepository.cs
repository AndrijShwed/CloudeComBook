using Dapper;
using CloudComBook.API.Data;
using CloudComBook.API.Repositories.Interfaces;
using CloudComBook.API.Services;
using CloudComBook.Shared.Models;

namespace CloudComBook.API.Repositories;

public class EnterpriseRepository : IEnterpriseRepository
{
    // @communityId = NULL означає «усі громади» (лише супер-адмін без вибраної громади).
    // Підприємство без зв'язку з вулицею бачить тільки супер-адмін.
    private const string Scope = @"(@communityId IS NULL OR e.villagestreetId IN (
            SELECT vs_s.id FROM villagestreet vs_s
            JOIN villages v_s ON v_s.id = vs_s.villageId
            WHERE v_s.community_id = @communityId))";

    private const string Select = @"
        SELECT e.id AS Id,
               e.name AS Name,
               e.owner AS Owner,
               e.employeesnumber AS EmployeesNumber,
               e.villagestreetId AS VillageStreetId,
               e.housenumber AS HouseNumber,
               v.name AS VillageName,
               s.name AS StreetName
        FROM enterprises e
        LEFT JOIN villagestreet vs ON e.villagestreetId = vs.id
        LEFT JOIN villages v ON vs.villageId = v.id
        LEFT JOIN streets s ON vs.streetId = s.id";

    private readonly DbConnectionFactory _db;
    private readonly ICurrentUser _user;

    public EnterpriseRepository(DbConnectionFactory db, ICurrentUser user)
    {
        _db = db;
        _user = user;
    }

    public async Task<IEnumerable<Enterprise>> GetAllAsync()
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<Enterprise>(
            $"{Select} WHERE {Scope} ORDER BY e.housenumber",
            new { communityId = _user.CommunityId });
    }

    public async Task<Enterprise?> GetByIdAsync(int id)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Enterprise>(
            $"{Select} WHERE e.id = @id AND {Scope}",
            new { id, communityId = _user.CommunityId });
    }

    public async Task<IEnumerable<Enterprise>> GetByVillageStreetIdAsync(int villageStreetId)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<Enterprise>(
            $"{Select} WHERE e.villagestreetId = @villageStreetId AND {Scope} ORDER BY e.housenumber",
            new { villageStreetId, communityId = _user.CommunityId });
    }

    public async Task<IEnumerable<Enterprise>> SearchAsync(string query)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<Enterprise>(
            $@"{Select}
              WHERE (e.housenumber LIKE @q OR e.owner LIKE @q OR e.name LIKE @q)
                AND {Scope}
              ORDER BY e.housenumber
              LIMIT 50",
            new { q = $"%{query}%", communityId = _user.CommunityId });
    }

    public async Task<IEnumerable<Enterprise>> SearchAsync(
        string? name = null,
        string? owner = null,
        int? villageId = null,
        int? streetId = null,
        string? houseNumber = null)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<Enterprise>(
            $@"{Select}
              WHERE {Scope}
                AND (@name IS NULL OR e.name LIKE CONCAT('%', @name, '%'))
                AND (@owner IS NULL OR e.owner LIKE CONCAT('%', @owner, '%'))
                AND (@villageId IS NULL OR vs.villageId = @villageId)
                AND (@streetId IS NULL OR vs.streetId = @streetId)
                AND (@houseNumber IS NULL OR e.housenumber = @houseNumber)
              ORDER BY e.name",
            new { communityId = _user.CommunityId, name, owner, villageId, streetId, houseNumber });
    }

    public async Task<int> CreateAsync(Enterprise enterprise)
    {
        // що адреса належить громаді користувача, перевіряє контролер (ICommunityGuard)
        using var conn = _db.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            @"INSERT INTO enterprises
              (name, owner, employeesnumber, villagestreetId, housenumber)
              VALUES
              (@Name, @Owner, @EmployeesNumber, @VillageStreetId, @HouseNumber);
              SELECT LAST_INSERT_ID();", enterprise);
    }

    public async Task<bool> UpdateAsync(Enterprise enterprise)
    {
        var args = new DynamicParameters(enterprise);
        args.Add("communityId", _user.CommunityId);

        using var conn = _db.CreateConnection();
        var rows = await conn.ExecuteAsync(
            $@"UPDATE enterprises e SET
              e.villagestreetId=@VillageStreetId,
              e.housenumber=@HouseNumber,
              e.name=@Name,
              e.owner=@Owner,
              e.employeesnumber=@EmployeesNumber
              WHERE e.id=@Id AND {Scope}", args);
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.ExecuteAsync(
            $"DELETE e FROM enterprises e WHERE e.id = @id AND {Scope}",
            new { id, communityId = _user.CommunityId });
        return rows > 0;
    }

    public async Task<bool> ExistsAsync(string name, int? villageStreetId, string? houseNumber, int? excludeId = null)
    {
        using var conn = _db.CreateConnection();
        var count = await conn.ExecuteScalarAsync<int>(
            $@"SELECT COUNT(*) FROM enterprises e
              WHERE (@excludeId IS NULL OR e.id != @excludeId)
                AND {Scope}
                AND (
                    LOWER(TRIM(e.name)) = LOWER(TRIM(@name))
                    OR (
                        e.villagestreetId = @villageStreetId
                        AND LOWER(TRIM(e.housenumber)) = LOWER(TRIM(@houseNumber))
                    )
                )",
            new { name, villageStreetId, houseNumber, excludeId, communityId = _user.CommunityId });
        return count > 0;
    }

    // залишено для сумісності зі старим інтерфейсом
    public async Task<bool> ExistsByNameAsync(string name) =>
        await ExistsAsync(name, null, null);
}