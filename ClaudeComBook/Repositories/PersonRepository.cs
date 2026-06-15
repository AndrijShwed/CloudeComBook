using Dapper;
using ClaudeComBook.API.Data;
using ClaudeComBook.API.Models;
using ClaudeComBook.API.Repositories.Interfaces;

namespace ClaudeComBook.API.Repositories;

public class PersonRepository : IPersonRepository
{
    private readonly DbConnectionFactory _db;

    public PersonRepository(DbConnectionFactory db) => _db = db;

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
    int? ageTo = null)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<Person>(
            @"SELECT 
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
            p.description AS Description,
            v.name AS VillageName,
            s.name AS StreetName
          FROM people p
          LEFT JOIN villagestreet vs ON p.villagestreetId = vs.id
          LEFT JOIN villages v ON vs.villageId = v.id
          LEFT JOIN streets s ON vs.streetId = s.id
          WHERE
            (@lastName IS NULL OR p.lastname LIKE CONCAT('%', @lastName, '%'))
            AND (@name IS NULL OR p.name LIKE CONCAT('%', @name, '%'))
            AND (@surname IS NULL OR p.surname LIKE CONCAT('%', @surname, '%'))
            AND (@sex IS NULL OR LOWER(p.sex) = LOWER(@sex))
            AND (@status IS NULL OR LOWER(p.status) LIKE CONCAT('%', LOWER(@status), '%'))
            AND (@registr IS NULL OR p.registr = @registr)
            AND (@houseNumb IS NULL OR p.numb_of_house = @houseNumb)
            AND (@villageId IS NULL OR p.villagestreetId IN (
                SELECT id FROM villagestreet WHERE villageId = @villageId))
            AND (@streetId IS NULL OR p.villagestreetId IN (
                SELECT id FROM villagestreet WHERE streetId = @streetId))
            AND (@ageFrom IS NULL OR TIMESTAMPDIFF(YEAR, p.date_of_birth, CURDATE()) >= @ageFrom)
            AND (@ageTo IS NULL OR TIMESTAMPDIFF(YEAR, p.date_of_birth, CURDATE()) <= @ageTo)
          ORDER BY p.lastname, p.name",
            new
            {
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
                ageTo
            });
    }

    public async Task<Person?> GetByIdAsync(int id)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Person>(
            @"SELECT 
            people_id AS PeopleId,
            lastname AS LastName,
            name AS Name,
            surname AS Surname,
            sex AS Sex,
            date_of_birth AS DateOfBirth,
            numb_of_house AS NumbOfHouse,
            passport AS Passport,
            id_kod AS IdKod,
            phone_numb AS PhoneNumber,
            status AS Status,
            registr AS Registr,
            m_date AS MDate,
            mil_ID AS MilID,
            villagestreetId AS VillageStreetId,
            description AS Description
           FROM people " +
            " WHERE people_id = @id", new { id });
    }

    public async Task<IEnumerable<Person>> GetByVillageStreetIdAsync(int villageStreetId)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<Person>(
             @"SELECT 
            people_id AS PeopleId,
            lastname AS LastName,
            name AS Name,
            surname AS Surname,
            sex AS Sex,
            date_of_birth AS DateOfBirth,
            numb_of_house AS NumbOfHouse,
            passport AS Passport,
            id_kod AS IdKod,
            phone_numb AS PhoneNumber,
            status AS Status,
            registr AS Registr,
            m_date AS MDate,
            mil_ID AS MilID,
            villagestreetId AS VillageStreetId,
            description AS Description
           FROM people 
              WHERE villagestreetId = @villageStreetId 
              ORDER BY lastname, name",
            new { villageStreetId });
    }

    public async Task<IEnumerable<Person>> SearchAsync(string query)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<Person>(
             @"SELECT 
            people_id AS PeopleId,
            lastname AS LastName,
            name AS Name,
            surname AS Surname,
            sex AS Sex,
            date_of_birth AS DateOfBirth,
            numb_of_house AS NumbOfHouse,
            passport AS Passport,
            id_kod AS IdKod,
            phone_numb AS PhoneNumber,
            status AS Status,
            registr AS Registr,
            m_date AS MDate,
            mil_ID AS MilID,
            villagestreetId AS VillageStreetId,
            description AS Description
           FROM people 
              WHERE lastname LIKE @q 
              OR name LIKE @q 
              OR surname LIKE @q
              OR id_kod LIKE @q
              OR phone_number LIKE @q
              ORDER BY lastname, name
              LIMIT 50",
            new { q = $"%{query}%" });
    }

    public async Task<int> CreateAsync(Person person)
    {
        using var conn = _db.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            @"INSERT INTO people 
              (lastname, name, surname, sex, date_of_birth, numb_of_house,
               passport, id_kod, phone_number, status, registr, m_date,
               mil_ID, villagestreetId, description)
              VALUES
              (@LastName, @Name, @Surname, @Sex, @DateOfBirth, @NumbOfHouse,
               @Passport, @IdKod, @PhoneNumber, @Status, @Registr, @MDate,
               @MilID, @VillageStreetId, @Description);
              SELECT LAST_INSERT_ID();", person);
    }

    public async Task<bool> UpdateAsync(Person person)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.ExecuteAsync(
            @"UPDATE people SET
              lastname=@LastName, name=@Name, surname=@Surname,
              sex=@Sex, date_of_birth=@DateOfBirth, numb_of_house=@NumbOfHouse,
              passport=@Passport, id_kod=@IdKod, phone_number=@PhoneNumber,
              status=@Status, registr=@Registr, m_date=@MDate,
              mil_ID=@MilID, villagestreetId=@VillageStreetId,
              description=@Description
              WHERE people_id=@PeopleId", person);
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.ExecuteAsync(
            "DELETE FROM people WHERE people_id = @id", new { id });
        return rows > 0;
    }
}