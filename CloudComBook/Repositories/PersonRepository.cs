using CloudComBook.API.Data;
using CloudComBook.API.Repositories.Interfaces;
using CloudComBook.API.Services;
using CloudComBook.Shared.Models;
using Dapper;

namespace CloudComBook.API.Repositories;

public class PersonRepository : IPersonRepository
{
    // Ізоляція за громадою. @communityId = NULL означає «усі громади» (лише супер-адмін без вибраної громади).
    // Запис без адреси (villagestreetId IS NULL) бачить тільки супер-адмін.
    private const string Scope = @"(@communityId IS NULL OR p.villagestreetId IN (
            SELECT vs_s.id FROM villagestreet vs_s
            JOIN villages v_s ON v_s.id = vs_s.villageId
            WHERE v_s.community_id = @communityId))";

    private const string PersonColumns = @"
            p.people_id AS PeopleId,
            p.lastname AS LastName,
            p.name AS Name,
            p.surname AS Surname,
            p.sex AS Sex,
            p.date_of_birth AS DateOfBirth,
            p.numb_of_house AS NumbOfHouse,
            p.passport AS Passport,
            p.id_kod AS IdKod,
            p.phone_numb AS PhoneNumber,
            p.status AS Status,
            p.registr AS Registr,
            p.m_date AS MDate,
            p.mil_ID AS MilID,
            p.villagestreetId AS VillageStreetId,
            p.description AS Description";

    private readonly DbConnectionFactory _db;
    private readonly ICurrentUser _user;

    public PersonRepository(DbConnectionFactory db, ICurrentUser user)
    {
        _db = db;
        _user = user;
    }

    public async Task<IEnumerable<Person>> GetAllAsync(
        string? lastName = null,
        string? name = null,
        string? surname = null,
        string? sex = null,
        string? status = null,
        string? registr = null,
        int? villageId = null,
        int? streetId = null,
        string? houseNumb = null,
        int? ageFrom = null,
        int? ageTo = null,
        int? statusYear = null,
        string? description = null)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<Person>(
            $@"SELECT {PersonColumns},
        v.name AS VillageName,
        s.name AS StreetName
      FROM people p
      LEFT JOIN villagestreet vs ON p.villagestreetId = vs.id
      LEFT JOIN villages v ON vs.villageId = v.id
      LEFT JOIN streets s ON vs.streetId = s.id
      WHERE
        {Scope}
        AND (@lastName IS NULL OR LOWER(p.lastname) LIKE CONCAT(LOWER(@lastName), '%'))
        AND (@name IS NULL OR LOWER(p.name) LIKE CONCAT(LOWER(@name), '%'))
        AND (@surname IS NULL OR LOWER(p.surname) LIKE CONCAT(LOWER(@surname), '%'))
        AND (@sex IS NULL OR LOWER(p.sex) = LOWER(@sex))
        AND (@status IS NULL OR LOWER(p.status) LIKE CONCAT(LOWER(@status), '%'))
        AND (@statusYear IS NULL OR YEAR(p.m_date) = @statusYear)
        AND (@registr IS NULL OR p.registr = @registr)
        AND (@houseNumb IS NULL OR p.numb_of_house = @houseNumb)
        AND (@villageId IS NULL OR p.villagestreetId IN (
            SELECT id FROM villagestreet WHERE villageId = @villageId))
        AND (@streetId IS NULL OR p.villagestreetId IN (
            SELECT id FROM villagestreet WHERE streetId = @streetId))
        AND (@ageFrom IS NULL OR (
                YEAR(CURDATE()) - YEAR(p.date_of_birth) -
                (DATE_FORMAT(CURDATE(), '%m%d') < DATE_FORMAT(p.date_of_birth, '%m%d'))
            ) >= @ageFrom)
        AND (@ageTo IS NULL OR (
                YEAR(CURDATE()) - YEAR(p.date_of_birth) -
                (DATE_FORMAT(CURDATE(), '%m%d') < DATE_FORMAT(p.date_of_birth, '%m%d'))
            ) < @ageTo)
      ORDER BY p.lastname, p.name",
            new
            {
                communityId = _user.CommunityId,
                lastName,
                name,
                surname,
                sex,
                status,
                registr,
                villageId,
                streetId,
                houseNumb,
                ageFrom,
                ageTo,
                statusYear,
                description
            });
    }

    public async Task<Person?> GetByIdAsync(int id)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Person>(
            $@"SELECT {PersonColumns},
            v.name AS VillageName,
            s.name AS StreetName
          FROM people p
          LEFT JOIN villagestreet vs ON p.villagestreetId = vs.id
          LEFT JOIN villages v ON vs.villageId = v.id
          LEFT JOIN streets s ON vs.streetId = s.id
          WHERE p.people_id = @id AND {Scope}",
            new { id, communityId = _user.CommunityId });
    }

    public async Task<IEnumerable<Person>> GetByVillageStreetIdAsync(int villageStreetId)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<Person>(
            $@"SELECT {PersonColumns}
           FROM people p
           WHERE p.villagestreetId = @villageStreetId AND {Scope}
           ORDER BY p.lastname, p.name",
            new { villageStreetId, communityId = _user.CommunityId });
    }

    public async Task<IEnumerable<Person>> SearchAsync(string query)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<Person>(
            $@"SELECT {PersonColumns}
           FROM people p
           WHERE (p.lastname LIKE @q
              OR p.name LIKE @q
              OR p.surname LIKE @q
              OR p.id_kod LIKE @q
              OR p.phone_numb LIKE @q)
              AND {Scope}
           ORDER BY p.lastname, p.name
           LIMIT 50",
            new { q = $"%{query}%", communityId = _user.CommunityId });
    }

    public async Task<int> CreateAsync(Person person)
    {
        // належність адреси громаді перевіряє контролер (ICommunityGuard)
        using var conn = _db.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            @"INSERT INTO people
              (lastname, name, surname, sex, date_of_birth, numb_of_house,
               passport, id_kod, phone_numb, status, registr, m_date,
               mil_ID, villagestreetId, description)
              VALUES
              (@LastName, @Name, @Surname, @Sex, @DateOfBirth, @NumbOfHouse,
               @Passport, @IdKod, @PhoneNumber, @Status, @Registr, @MDate,
               @MilID, @VillageStreetId, @Description);
              SELECT LAST_INSERT_ID();", person);
    }

    public async Task<bool> UpdateAsync(Person person)
    {
        var args = new DynamicParameters(person);
        args.Add("communityId", _user.CommunityId);

        using var conn = _db.CreateConnection();
        var rows = await conn.ExecuteAsync(
            $@"UPDATE people p SET
              p.lastname=@LastName, p.name=@Name, p.surname=@Surname,
              p.sex=@Sex, p.date_of_birth=@DateOfBirth, p.numb_of_house=@NumbOfHouse,
              p.passport=@Passport, p.id_kod=@IdKod, p.phone_numb=@PhoneNumber,
              p.status=@Status, p.registr=@Registr, p.m_date=@MDate,
              p.mil_ID=@MilID, p.villagestreetId=@VillageStreetId,
              p.description=@Description
              WHERE p.people_id=@PeopleId AND {Scope}", args);
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.ExecuteAsync(
            $"DELETE p FROM people p WHERE p.people_id = @id AND {Scope}",
            new { id, communityId = _user.CommunityId });
        return rows > 0;
    }

    public async Task<Dictionary<string, int>> GetPopulationByVillageAsync()
    {
        using var conn = _db.CreateConnection();
        var result = await conn.QueryAsync<(string Village, int Count)>(
            $@"SELECT v.name AS Village, COUNT(*) AS Count
          FROM people p
          JOIN villagestreet vs ON p.villagestreetId = vs.id
          JOIN villages v ON vs.villageId = v.id
          WHERE (p.status IS NULL OR p.status != 'помер')
            AND p.registr = 'так'
            AND {Scope}
          GROUP BY v.name",
            new { communityId = _user.CommunityId });

        return result.ToDictionary(r => r.Village, r => r.Count);
    }

    public async Task<bool> ExistsAsync(string lastName, string name, string? surname, DateTime? dateOfBirth)
    {
        using var conn = _db.CreateConnection();
        var count = await conn.ExecuteScalarAsync<int>(
            $@"SELECT COUNT(*) FROM people p
          WHERE p.lastname = @lastName
          AND p.name = @name
          AND (@surname IS NULL OR p.surname = @surname)
          AND (@dateOfBirth IS NULL OR p.date_of_birth = @dateOfBirth)
          AND {Scope}",
            new { lastName, name, surname, dateOfBirth, communityId = _user.CommunityId });
        return count > 0;
    }

    public async Task<IEnumerable<Person>> GetByAddressAsync(int villageStreetId, string numbOfHouse, int? id)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<Person>(
            $@"SELECT
            p.people_id AS PeopleId,
            p.lastname AS LastName,
            p.name AS Name,
            p.surname AS Surname,
            p.date_of_birth AS DateOfBirth,
            p.registr AS Registr,
            p.passport AS Passport
          FROM people p
          WHERE p.villagestreetId = @villageStreetId
          AND p.numb_of_house = @numbOfHouse
          AND p.registr = 'так'
          AND (@id IS NULL OR p.people_id <> @id)
          AND {Scope}
          ORDER BY p.lastname, p.name",
            new { villageStreetId, numbOfHouse, id, communityId = _user.CommunityId });
    }
}