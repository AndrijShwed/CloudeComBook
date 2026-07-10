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
        public async Task<IEnumerable<Enterprise>> SearchAsync(
            string? name = null,
            string? owner = null,
            int? villageId = null,
            int? streetId = null,
            string? houseNumber = null)
        {
            using var conn = _db.CreateConnection();
            return await conn.QueryAsync<Enterprise>(
                @"SELECT 
            e.id AS Id,
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
          LEFT JOIN streets s ON vs.streetId = s.id
          WHERE
            (@name IS NULL OR e.name LIKE CONCAT('%', @name, '%'))
            AND (@owner IS NULL OR e.owner LIKE CONCAT('%', @owner, '%'))
            AND (@villageId IS NULL OR vs.villageId = @villageId)
            AND (@streetId IS NULL OR vs.streetId = @streetId)
            AND (@houseNumber IS NULL OR e.housenumber = @houseNumber)
          ORDER BY e.name",
                new { name, owner, villageId, streetId, houseNumber });
        }
    }
}
