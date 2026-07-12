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
    public async Task<bool> ExistsAsync(string lastName, string name, string? surname, string village)
    {
        using var conn = _db.CreateConnection();
        var count = await conn.ExecuteScalarAsync<int>(
            @"SELECT COUNT(*) FROM anymals 
          WHERE lastname = @lastName 
          AND name = @name
          AND village = @village
          AND (@surname IS NULL OR surname = @surname)",
            new { lastName, name, surname, village });
        return count > 0;
    }
    public async Task<IEnumerable<Anymal>> SearchAsync(
    string? lastName = null,
    string? name = null,
    string? surname = null,
    string? village = null)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<Anymal>(
            @"SELECT 
            anymalsId AS AnymalsId,
            lastname AS LastName,
            name AS Name,
            surname AS Surname,
            village AS Village,
            anymals AS Anymals,
            covs AS Covs,
            pigs AS Pigs,
            sheeps AS Sheeps,
            goats AS Goats,
            horses AS Horses,
            birds AS Birds,
            rabbits AS Rabbits,
            beeses AS Beeses
          FROM anymals
          WHERE
            (@lastName IS NULL OR lastname LIKE CONCAT('%', @lastName, '%'))
            AND (@name IS NULL OR name LIKE CONCAT('%', @name, '%'))
            AND (@surname IS NULL OR surname LIKE CONCAT('%', @surname, '%'))
            AND (@village IS NULL OR village = @village)
          ORDER BY lastname, name",
            new { lastName, name, surname, village });
    }
    public async Task<IEnumerable<dynamic>> GetStatisticsByVillageAsync()
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync(
            @"SELECT 
            village AS Village,
            SUM(covs) AS Covs,
            SUM(pigs) AS Pigs,
            SUM(sheeps) AS Sheeps,
            SUM(goats) AS Goats,
            SUM(horses) AS Horses,
            SUM(birds) AS Birds,
            SUM(rabbits) AS Rabbits,
            SUM(beeses) AS Beeses,
            SUM(anymals) AS Anymals
          FROM anymals
          GROUP BY village
          ORDER BY village");
    }
}