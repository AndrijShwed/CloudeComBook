using Dapper;
using ClaudeComBook.API.Data;
using ClaudeComBook.API.Models;
using ClaudeComBook.API.Repositories.Interfaces;

namespace ClaudeComBook.API.Repositories;

public class PopulationSnapshotRepository : IPopulationSnapshotRepository
{
    private readonly DbConnectionFactory _db;

    public PopulationSnapshotRepository(DbConnectionFactory db) => _db = db;

    public async Task<IEnumerable<PopulationSnapshot>> GetAllAsync()
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<PopulationSnapshot>(
            @"SELECT 
            id,
            settlement_name AS SettlementName,
            year,
            population,
            created_at AS CreatedAt
          FROM population_snapshot 
          ORDER BY year, id");
    }

    public async Task<PopulationSnapshot?> GetByIdAsync(int id)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<PopulationSnapshot>(
             @"SELECT 
            id,
            settlement_name AS SettlementName,
            year,
            population,
            created_at AS CreatedAt
            FROM population_snapshot WHERE id = @id", new { id });
    }

    public async Task<IEnumerable<PopulationSnapshot>> SearchAsync(string query)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<PopulationSnapshot>(
             @"SELECT 
            id,
            settlement_name AS SettlementName,
            year,
            population,
            created_at AS CreatedAt
            FROM population_snapshot 
              WHERE year LIKE @q
              ORDER BY year
              LIMIT 50",
            new { q = $"%{query}%" });
    }

    public async Task<int> CreateAsync(PopulationSnapshot populationSnapshot)
    {
        using var conn = _db.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            @"INSERT INTO population_snapshot 
              (settlement_name, year, population, created_at)
              VALUES
              (@Settlemet_name, @Year, @Population, @Created_at);
              SELECT LAST_INSERT_ID();", populationSnapshot);
    }

    public async Task<bool> UpdateAsync(PopulationSnapshot populationSnapshot)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.ExecuteAsync(
            @"UPDATE population_snapshot SET
              settlement_name=@settlement_name,
              year=@Year,
              population=@Population,
              created_at=@Created_at
              WHERE id=@Id", populationSnapshot);
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.ExecuteAsync(
            "DELETE FROM population_snapshot WHERE id = @id", new { id });
        return rows > 0;
    }
}

