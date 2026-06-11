using Dapper;
using ClaudeComBook.API.Data;
using ClaudeComBook.API.Models;
using ClaudeComBook.API.Repositories.Interfaces;


namespace ClaudeComBook.API.Repositories
{
    public class EnterpriseRepository : IEnterpriseRepository
    {
        private readonly DbConnectionFactory _db;

        public EnterpriseRepository(DbConnectionFactory db) => _db = db;
        public async Task<int> CreateAsync(Enterprise enterprise)
        {
            using var conn = _db.CreateConnection();
            return await conn.ExecuteScalarAsync<int>(
                @"INSERT INTO enterprises 
              (name, owner, employeesnumber,villagestreetId, housenumber)
              VALUES
              (@Name, @Owner, @Employeesnumber, @VillageStreetId, @Housenumber);
              SELECT LAST_INSERT_ID();", enterprise);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var conn = _db.CreateConnection();
            var rows = await conn.ExecuteAsync(
                "DELETE FROM enterprises WHERE id = @id", new { id });
            return rows > 0;
        }

        public async Task<IEnumerable<Enterprise>> GetAllAsync()
        {
            using var conn = _db.CreateConnection();
            return await conn.QueryAsync<Enterprise>(
                "SELECT * FROM enterprises ORDER BY housenumber");
        }

        public async Task<Enterprise?> GetByIdAsync(int id)
        {
            using var conn = _db.CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<Enterprise>(
                "SELECT * FROM enterprises WHERE id = @id", new { id });
        }

        public async Task<IEnumerable<Enterprise>> GetByVillageStreetIdAsync(int villageStreetId)
        {
            using var conn = _db.CreateConnection();
            return await conn.QueryAsync<Enterprise>(
                @"SELECT * FROM enterprises 
              WHERE villagestreetId = @villageStreetId 
              ORDER BY housenumber",
                new { villageStreetId });
        }

        public async Task<IEnumerable<Enterprise>> SearchAsync(string query)
        {
            using var conn = _db.CreateConnection();
            return await conn.QueryAsync<Enterprise>(
                @"SELECT * FROM enterprises 
              WHERE housenumber LIKE @q
              OR owner LIKE @q
              OR name LIKE @q
              ORDER BY housenumber
              LIMIT 50",
                new { q = $"%{query}%" });
        }

        public async Task<bool> UpdateAsync(Enterprise enterprise)
        {
            using var conn = _db.CreateConnection();
            var rows = await conn.ExecuteAsync(
                @"UPDATE enterprises SET
              villagestreetId=@VillageStreetId,
              housenumber=@HouseNumber,
              name=@Name,
              owner=@Owner,
              employeesnumber=@Employeesnumber
              WHERE id=@Id", enterprise);
            return rows > 0;
        }
    }
}
