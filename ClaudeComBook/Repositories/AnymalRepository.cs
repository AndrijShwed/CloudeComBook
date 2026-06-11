using Dapper;
using ClaudeComBook.API.Data;
using ClaudeComBook.API.Models;
using ClaudeComBook.API.Repositories.Interfaces;

namespace ClaudeComBook.API.Repositories;

public class AnymalRepository : IAnymalRepository
{
    private readonly DbConnectionFactory _db;

    public AnymalRepository(DbConnectionFactory db) => _db = db;

    public async Task<IEnumerable<Anymal>> GetAllAsync()
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<Anymal>(
            "SELECT * FROM anymals ORDER BY lastname, name");
    }

    public async Task<Anymal?> GetByIdAsync(int id)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Anymal>(
            "SELECT * FROM anymals WHERE anymalsId = @id", new { id });
    }

    public async Task<IEnumerable<Anymal>> SearchAsync(string query)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<Anymal>(
            @"SELECT * FROM anymals
              WHERE lastname LIKE @q
              OR name LIKE @q
              OR village LIKE @q
              ORDER BY lastname, name
              LIMIT 50",
            new { q = $"%{query}%" });
    }

    public async Task<int> CreateAsync(Anymal anymal)
    {
        using var conn = _db.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            @"INSERT INTO anymals
              (lastname, name, surname, village, anymals, covs, pigs,
               sheeps, goats, horses, birds, rabbits, beeses)
              VALUES
              (@LastName, @Name, @Surname, @Village, @Anymals, @Covs, @Pigs,
               @Sheeps, @Goats, @Horses, @Birds, @Rabbits, @Beeses);
              SELECT LAST_INSERT_ID();", anymal);
    }

    public async Task<bool> UpdateAsync(Anymal anymal)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.ExecuteAsync(
            @"UPDATE anymals SET
              lastname=@LastName, name=@Name, surname=@Surname,
              village=@Village, anymals=@Anymals, covs=@Covs,
              pigs=@Pigs, sheeps=@Sheeps, goats=@Goats,
              horses=@Horses, birds=@Birds, rabbits=@Rabbits,
              beeses=@Beeses
              WHERE anymalsId=@AnymalsId", anymal);
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.ExecuteAsync(
            "DELETE FROM anymals WHERE anymalsId = @id", new { id });
        return rows > 0;
    }
}