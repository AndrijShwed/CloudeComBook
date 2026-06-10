using Dapper;
using ClaudeComBook.API.Data;
using ClaudeComBook.API.Models;
using ClaudeComBook.API.Repositories.Interfaces;

namespace ClaudeComBook.API.Repositories;

public class VillageStreetRepository : IVillageStreetRepository
{
    private readonly DbConnectionFactory _db;

    public VillageStreetRepository(DbConnectionFactory db) => _db = db;

    public async Task<IEnumerable<VillageStreet>> GetAllAsync()
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<VillageStreet>(
            "SELECT * FROM villagestreet ORDER BY id");
    }

    public async Task<VillageStreet?> GetByIdAsync(int id)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<VillageStreet>(
            "SELECT * FROM villagestreet WHERE id = @id", new { id });
    }

    public async Task<IEnumerable<VillageStreet>> GetByVillageIdAsync(int villageId)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<VillageStreet>(
            @"SELECT vs.*, v.name AS VillageName, s.name AS StreetName
              FROM villagestreet vs
              LEFT JOIN villages v ON vs.villageId = v.id
              LEFT JOIN streets s ON vs.streetId = s.id
              WHERE vs.villageId = @villageId AND vs.isActive = 1
              ORDER BY s.name",
            new { villageId });
    }

    public async Task<IEnumerable<VillageStreet>> GetByStreetIdAsync(int streetId)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<VillageStreet>(
            "SELECT * FROM villagestreet WHERE streetId = @streetId",
            new { streetId });
    }

    public async Task<int> CreateAsync(VillageStreet vs)
    {
        using var conn = _db.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            @"INSERT INTO villagestreet (villageId, streetId, oldStreetId, isActive, renameDate, fileData)
              VALUES (@VillageId, @StreetId, @OldStreetId, @IsActive, @RenameDate, @FileData);
              SELECT LAST_INSERT_ID();", vs);
    }

    public async Task<bool> UpdateAsync(VillageStreet vs)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.ExecuteAsync(
            @"UPDATE villagestreet SET villageId=@VillageId, streetId=@StreetId,
              oldStreetId=@OldStreetId, isActive=@IsActive,
              renameDate=@RenameDate, fileData=@FileData
              WHERE id=@Id", vs);
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.ExecuteAsync(
            "DELETE FROM villagestreet WHERE id = @id", new { id });
        return rows > 0;
    }
}