using Dapper;
using CloudComBook.API.Data;
using CloudComBook.API.Services;
using CloudComBook.Shared.Models;
using CloudComBook.API.Repositories.Interfaces;

namespace CloudComBook.API.Repositories;

public class VillageStreetRepository : IVillageStreetRepository
{
    // @communityId = NULL означає «усі громади» (лише супер-адмін без вибраної громади)
    private const string Scope = @"(@communityId IS NULL OR vs.villageId IN (
            SELECT v_s.id FROM villages v_s WHERE v_s.community_id = @communityId))";

    private readonly DbConnectionFactory _db;
    private readonly ICurrentUser _user;

    public VillageStreetRepository(DbConnectionFactory db, ICurrentUser user)
    {
        _db = db;
        _user = user;
    }

    public async Task<IEnumerable<VillageStreet>> GetAllAsync()
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<VillageStreet>(
            $@"SELECT vs.id AS Id, vs.villageId AS VillageId, vs.streetId AS StreetId,
            vs.oldStreetId AS OldStreetId, vs.isActive AS IsActive,
            vs.renameDate AS RenameDate,
            v.name AS VillageName,
            s.name AS StreetName,
            os.name AS OldStreetName,
            vs.fileData AS FileData
          FROM villagestreet vs
          LEFT JOIN villages v ON vs.villageId = v.id
          LEFT JOIN streets s ON vs.streetId = s.id
          LEFT JOIN streets os ON vs.oldStreetId = os.id
          WHERE {Scope}
          ORDER BY v.name, s.name",
            new { communityId = _user.CommunityId });
    }

    public async Task<VillageStreet?> GetByIdAsync(int id)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<VillageStreet>(
            $"SELECT vs.* FROM villagestreet vs WHERE vs.id = @id AND {Scope}",
            new { id, communityId = _user.CommunityId });
    }

    public async Task<IEnumerable<VillageStreet>> GetByVillageIdAsync(int villageId)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<VillageStreet>(
            $@"SELECT vs.*, v.name AS VillageName, s.name AS StreetName
              FROM villagestreet vs
              LEFT JOIN villages v ON vs.villageId = v.id
              LEFT JOIN streets s ON vs.streetId = s.id
              WHERE vs.villageId = @villageId AND vs.isActive = 1 AND {Scope}
              ORDER BY s.name",
            new { villageId, communityId = _user.CommunityId });
    }

    public async Task<IEnumerable<VillageStreet>> GetByStreetIdAsync(int streetId)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<VillageStreet>(
            $"SELECT vs.* FROM villagestreet vs WHERE vs.streetId = @streetId AND {Scope}",
            new { streetId, communityId = _user.CommunityId });
    }

    public async Task<int> CreateAsync(VillageStreet vs)
    {
        // що село й вулиця належать громаді користувача, перевіряє контролер (ICommunityGuard)
        using var conn = _db.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            @"INSERT INTO villagestreet (villageId, streetId, oldStreetId, isActive, renameDate, fileData)
              VALUES (@VillageId, @StreetId, @OldStreetId, @IsActive, @RenameDate, @FileData);
              SELECT LAST_INSERT_ID();", vs);
    }

    public async Task<bool> UpdateAsync(VillageStreet item)
    {
        var args = new DynamicParameters(item);
        args.Add("communityId", _user.CommunityId);

        using var conn = _db.CreateConnection();
        var rows = await conn.ExecuteAsync(
            $@"UPDATE villagestreet vs SET vs.villageId=@VillageId, vs.streetId=@StreetId,
              vs.oldStreetId=@OldStreetId, vs.isActive=@IsActive,
              vs.renameDate=@RenameDate, vs.fileData=@FileData
              WHERE vs.id=@Id AND {Scope}", args);
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.ExecuteAsync(
            $"DELETE vs FROM villagestreet vs WHERE vs.id = @id AND {Scope}",
            new { id, communityId = _user.CommunityId });
        return rows > 0;
    }

    public async Task<bool> UpdateFileAsync(int id, byte[] fileData)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.ExecuteAsync(
            $"UPDATE villagestreet vs SET vs.fileData = @fileData WHERE vs.id = @id AND {Scope}",
            new { id, fileData, communityId = _user.CommunityId });
        return rows > 0;
    }

    public async Task<bool> RenameStreetAsync(int villageId, int oldStreetId,
        int newStreetId, DateTime? renameDate, byte[]? fileData)
    {
        // якщо перейменування колись з'явиться в контролері, newStreetId теж перевіряйте через StreetAllowedAsync
        using var conn = _db.CreateConnection();
        var rows = await conn.ExecuteAsync(
            $@"UPDATE villagestreet vs
          SET vs.streetId = @newStreetId,
              vs.oldStreetId = @oldStreetId,
              vs.renameDate = @renameDate,
              vs.fileData = @fileData
          WHERE vs.villageId = @villageId
          AND vs.streetId = @oldStreetId
          AND {Scope}",
            new { villageId, oldStreetId, newStreetId, renameDate, fileData, communityId = _user.CommunityId });
        return rows > 0;
    }
}