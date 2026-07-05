using Dapper;
using ClaudeComBook.API.Data;
using ClaudeComBook.API.Models;
using ClaudeComBook.API.Repositories.Interfaces;

namespace ClaudeComBook.API.Repositories;

public class HouseRepository : IHouseRepository
{
    private readonly DbConnectionFactory _db;

    public HouseRepository(DbConnectionFactory db) => _db = db;

    public async Task<IEnumerable<House>> GetAllAsync()
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<House>(
            "SELECT * FROM houses ORDER BY numb_of_house");
    }

    public async Task<House?> GetByIdAsync(int id)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<House>(
            "SELECT * FROM houses WHERE idhouses = @id", new { id });
    }

    public async Task<IEnumerable<House>> GetByVillageStreetIdAsync(int villageStreetId)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<House>(
            @"SELECT * FROM houses 
              WHERE villagestreetId = @villageStreetId 
              ORDER BY numb_of_house",
            new { villageStreetId });
    }

    public async Task<IEnumerable<House>> SearchAsync(string query)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<House>(
            @"SELECT * FROM houses 
              WHERE numb_of_house LIKE @q
              OR lastname LIKE @q
              OR name LIKE @q
              ORDER BY numb_of_house
              LIMIT 50",
            new { q = $"%{query}%" });
    }

    public async Task<int> CreateAsync(House house)
    {
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
        using var conn = _db.CreateConnection();
        var rows = await conn.ExecuteAsync(
            @"UPDATE houses SET
              villagestreetId=@VillageStreetId,
              numb_of_house=@NumbOfHouse,
              lastname=@LastName,
              name=@Name,
              surname=@Surname,
              totalArea=@TotalArea,
              livingArea=@LivingArea,
              total_of_rooms=@TotalOfRooms
              WHERE idhouses=@IdHouses", house);
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.ExecuteAsync(
            "DELETE FROM houses WHERE idhouses = @id", new { id });
        return rows > 0;
    }
    public async Task<bool> ExistsAsync(int villageStreetId, string numbOfHouse)
    {
        using var conn = _db.CreateConnection();
        var count = await conn.ExecuteScalarAsync<int>(
            @"SELECT COUNT(*) FROM houses 
          WHERE villagestreetId = @villageStreetId 
          AND numb_of_house = @numbOfHouse",
            new { villageStreetId, numbOfHouse });
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
                    @"SELECT 
                        h.idhouses AS IdHouses,
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
                      LEFT JOIN streets s ON vs.streetId = s.id
                      WHERE
                        (@villageId IS NULL OR vs.villageId = @villageId)
                        AND (@streetId IS NULL OR vs.streetId = @streetId)
                        AND (@houseNumber IS NULL OR h.numb_of_house = @houseNumber)
                        AND (@lastName IS NULL OR h.lastname LIKE CONCAT('%', @lastName, '%'))
                        AND (@name IS NULL OR h.name LIKE CONCAT('%', @name, '%'))
                        AND (@surname IS NULL OR h.surname LIKE CONCAT('%', @surname, '%'))
                      ORDER BY v.name, s.name, h.numb_of_house",
                    new { villageId, streetId, houseNumber, lastName, name, surname });
    }
}
