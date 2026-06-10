using Dapper;
using ClaudeComBook.API.Data;
using ClaudeComBook.API.Models;
using ClaudeComBook.API.Repositories.Interfaces;

namespace ClaudeComBook.API.Repositories;

public class PersonRepository : IPersonRepository
{
    private readonly DbConnectionFactory _db;

    public PersonRepository(DbConnectionFactory db) => _db = db;

    public async Task<IEnumerable<Person>> GetAllAsync()
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<Person>(
            "SELECT * FROM people ORDER BY lastname, name");
    }

    public async Task<Person?> GetByIdAsync(int id)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Person>(
            "SELECT * FROM people WHERE people_id = @id", new { id });
    }

    public async Task<IEnumerable<Person>> GetByVillageStreetIdAsync(int villageStreetId)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<Person>(
            @"SELECT * FROM people 
              WHERE villagestreetId = @villageStreetId 
              ORDER BY lastname, name",
            new { villageStreetId });
    }

    public async Task<IEnumerable<Person>> SearchAsync(string query)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<Person>(
            @"SELECT * FROM people 
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