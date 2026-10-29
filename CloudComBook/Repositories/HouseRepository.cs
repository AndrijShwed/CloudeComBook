using Dapper;
using CloudComBook.API.Data;
using CloudComBook.API.Repositories.Interfaces;
using CloudComBook.API.Services;
using CloudComBook.Shared.Models;

namespace CloudComBook.API.Repositories;

public class HouseRepository : IHouseRepository
{
    // @communityId = NULL означає «усі громади» (лише супер-адмін без вибраної громади).
    // Будинок без зв'язку з вулицею бачить тільки супер-адмін.
    private const string Scope = @"(@communityId IS NULL OR h.villagestreetId IN (
            SELECT vs_s.id FROM villagestreet vs_s
            JOIN villages v_s ON v_s.id = vs_s.villageId
            WHERE v_s.community_id = @communityId))";

    private const string Select = @"
        SELECT h.idhouses AS IdHouses,
               h.villagestreetId AS VillageStreetId,
               h.numb_of_house AS NumbOfHouse,
               h.lastname AS LastName,
               h.name AS Name,
               h.surname AS Surname,
               h.totalArea AS TotalArea,
               h.livingArea AS LivingArea,
               h.total_of_rooms AS TotalOfRooms,
               v.name AS VillageName,
               s.name AS StreetName
        FROM houses h
        LEFT JOIN villagestreet vs ON h.villagestreetId = vs.id
        LEFT JOIN villages v ON vs.villageId = v.id
        LEFT JOIN streets s ON vs.streetId = s.id";

    private readonly DbConnectionFactory _db;
    private readonly ICurrentUser _user;

    public HouseRepository(DbConnectionFactory db, ICurrentUser user)
    {
        _db = db;
        _user = user;
    }

    public async Task<IEnumerable<House>> GetAllAsync()
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<House>(
            $"{Select} WHERE {Scope} ORDER BY h.numb_of_house",
            new { communityId = _user.CommunityId });
    }

    public async Task<House?> GetByIdAsync(int id)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<House>(
            $"{Select} WHERE h.idhouses = @id AND {Scope}",
            new { id, communityId = _user.CommunityId });
    }

    public async Task<IEnumerable<House>> GetByVillageStreetIdAsync(int villageStreetId)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<House>(
            $"{Select} WHERE h.villagestreetId = @villageStreetId AND {Scope} ORDER BY h.numb_of_house",
            new { villageStreetId, communityId = _user.CommunityId });
    }

    public async Task<IEnumerable<House>> GetByVillageIdAsync(int villageId)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<House>(
            $"{Select} WHERE vs.villageId = @villageId AND {Scope} ORDER BY s.name, h.numb_of_house",
            new { villageId, communityId = _user.CommunityId });
    }

    public async Task<int> CreateAsync(House house)
    {
        // що адреса належить громаді користувача, перевіряє контролер (ICommunityGuard)
        using var conn = _db.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            @"INSERT INTO houses
              (villagestreetId, numb_of_house, lastname, name, surname,
               totalArea, livingArea, total_of_rooms)
              VALUES
              (@VillageStreetId, @NumbOfHouse, @LastName, @Name, @Surname,
               @TotalArea, @LivingArea, @TotalOfRooms);
              SELECT LAST_INSERT_ID();", house);
    }

    public async Task<bool> UpdateAsync(House house)
    {
        var args = new DynamicParameters(house);
        args.Add("communityId", _user.CommunityId);

        using var conn = _db.CreateConnection();
        var rows = await conn.ExecuteAsync(
            $@"UPDATE houses h SET
              h.villagestreetId=@VillageStreetId,
              h.numb_of_house=@NumbOfHouse,
              h.lastname=@LastName,
              h.name=@Name,
              h.surname=@Surname,
              h.totalArea=@TotalArea,
              h.livingArea=@LivingArea,
              h.total_of_rooms=@TotalOfRooms
              WHERE h.idhouses=@IdHouses AND {Scope}", args);
        return rows > 0;
    }

    public async Task<bool> HasRegisteredPeopleAsync(int id)
    {
        using var conn = _db.CreateConnection();
        var count = await conn.ExecuteScalarAsync<int>(
            $@"SELECT COUNT(*) FROM people p
          INNER JOIN houses h ON h.villagestreetId = p.villagestreetId
                              AND h.numb_of_house = p.numb_of_house
          WHERE h.idhouses = @id AND {Scope}",
            new { id, communityId = _user.CommunityId });
        return count > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        if (await HasRegisteredPeopleAsync(id))
            throw new InvalidOperationException("Неможливо видалити будинок: у ньому зареєстровані люди.");

        using var conn = _db.CreateConnection();
        var rows = await conn.ExecuteAsync(
            $"DELETE h FROM houses h WHERE h.idhouses = @id AND {Scope}",
            new { id, communityId = _user.CommunityId });
        return rows > 0;
    }

    public async Task<bool> ExistsAsync(int villageStreetId, string numbOfHouse)
    {
        using var conn = _db.CreateConnection();
        var count = await conn.ExecuteScalarAsync<int>(
            $@"SELECT COUNT(*) FROM houses h
          WHERE h.villagestreetId = @villageStreetId
          AND h.numb_of_house = @numbOfHouse
          AND {Scope}",
            new { villageStreetId, numbOfHouse, communityId = _user.CommunityId });
        return count > 0;
    }

    public async Task<IEnumerable<House>> SearchAsync(
        int? villageId = null,
        int? streetId = null,
        string? houseNumber = null,
        string? lastName = null,
        string? name = null,
        string? surname = null)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<House>(
            $@"{Select}
              WHERE {Scope}
                AND (@villageId IS NULL OR vs.villageId = @villageId)
                AND (@streetId IS NULL OR vs.streetId = @streetId)
                AND (@houseNumber IS NULL OR h.numb_of_house = @houseNumber)
                AND (@lastName IS NULL OR h.lastname LIKE CONCAT('%', @lastName, '%'))
                AND (@name IS NULL OR h.name LIKE CONCAT('%', @name, '%'))
                AND (@surname IS NULL OR h.surname LIKE CONCAT('%', @surname, '%'))
              ORDER BY v.name, s.name, h.numb_of_house",
            new
            {
                communityId = _user.CommunityId,
                villageId,
                streetId,
                houseNumber,
                lastName,
                name,
                surname
            });
    }

    public async Task<IEnumerable<(string Village, decimal TotalArea, decimal LivingArea)>> GetAreaByVillageAsync()
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<(string Village, decimal TotalArea, decimal LivingArea)>(
            $@"SELECT v.name AS Village,
            SUM(h.totalArea) AS TotalArea,
            SUM(h.livingArea) AS LivingArea
            FROM houses h
            LEFT JOIN villagestreet vs ON h.villagestreetId = vs.id
            LEFT JOIN villages v ON vs.villageId = v.id
            WHERE {Scope}
            GROUP BY v.id, v.name
            ORDER BY v.name",
            new { communityId = _user.CommunityId });
    }

    public async Task<IEnumerable<dynamic>> GetRoomCountByVillageAsync()
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync(
            $@"SELECT
            v.name AS Village,
            SUM(CASE WHEN h.total_of_rooms = 1 THEN 1 ELSE 0 END) AS OneRoom,
            SUM(CASE WHEN h.total_of_rooms = 2 THEN 1 ELSE 0 END) AS TwoRooms,
            SUM(CASE WHEN h.total_of_rooms = 3 THEN 1 ELSE 0 END) AS ThreeRooms,
            SUM(CASE WHEN h.total_of_rooms = 4 THEN 1 ELSE 0 END) AS FourRooms,
            SUM(CASE WHEN h.total_of_rooms = 5 THEN 1 ELSE 0 END) AS FiveRooms,
            SUM(CASE WHEN h.total_of_rooms = 6 THEN 1 ELSE 0 END) AS SixRooms,
            SUM(CASE WHEN h.total_of_rooms > 6 THEN 1 ELSE 0 END) AS MoreThanSix,
            SUM(CASE WHEN h.total_of_rooms BETWEEN 1 AND 6 THEN 1 ELSE 0 END) +
            SUM(CASE WHEN h.total_of_rooms > 6 THEN 1 ELSE 0 END) AS Total
          FROM houses h
          LEFT JOIN villagestreet vs ON h.villagestreetId = vs.id
          LEFT JOIN villages v ON vs.villageId = v.id
          WHERE {Scope}
          GROUP BY v.id, v.name
          ORDER BY v.name",
            new { communityId = _user.CommunityId });
    }
}