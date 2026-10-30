using CloudComBook.API.Data;
using CloudComBook.API.Repositories.Interfaces;
using CloudComBook.API.Services;
using CloudComBook.Shared.Models;
using Dapper;

namespace CloudComBook.API.Repositories;

public class PlotRepository : IPlotRepository
{
    // @communityId = NULL означає «усі громади» (лише супер-адмін без вибраної громади)
    private const string Scope = "(@communityId IS NULL OR p.community_id = @communityId)";

    private const string Select = @"
        SELECT p.id AS Id, p.fullname AS FullName, p.village AS Village, p.street AS Street,
               p.housenumb AS HouseNumb, p.fieldnumber AS FieldNumber, p.plottype AS PlotType,
               p.plotnumber AS PlotNumber, p.plotarea AS PlotArea, p.cadastr AS Cadastr,
               p.tenant AS Tenant, p.url AS Url, p.community_id AS CommunityId
        FROM plot p";

    private readonly DbConnectionFactory _db;
    private readonly ICurrentUser _user;

    public PlotRepository(DbConnectionFactory db, ICurrentUser user)
    {
        _db = db;
        _user = user;
    }

    public async Task<IEnumerable<Plot>> GetAllAsync()
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<Plot>(
            $"{Select} WHERE {Scope} ORDER BY p.village",
            new { communityId = _user.CommunityId });
    }

    public async Task<Plot?> GetByIdAsync(int id)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Plot>(
            $"{Select} WHERE p.id = @id AND {Scope}",
            new { id, communityId = _user.CommunityId });
    }

    public async Task<IEnumerable<Plot>> SearchAsync(string query)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<Plot>(
            $@"{Select}
              WHERE (p.village LIKE @q OR p.street LIKE @q OR p.fullname LIKE @q
                     OR p.cadastr LIKE @q OR p.tenant LIKE @q)
                AND {Scope}
              ORDER BY p.village
              LIMIT 50",
            new { q = $"%{query}%", communityId = _user.CommunityId });
    }

    public async Task<int> CreateAsync(Plot plot)
    {
        // CommunityId виставляє контролер із токена, значенню з клієнта не довіряємо
        using var conn = _db.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            @"INSERT INTO plot
              (fullname, village, street, housenumb, fieldnumber, plottype, plotnumber,
               plotarea, cadastr, tenant, url, community_id)
              VALUES
              (@FullName, @Village, @Street, @HouseNumb, @FieldNumber, @PlotType, @PlotNumber,
               @PlotArea, @Cadastr, @Tenant, @Url, @CommunityId);
              SELECT LAST_INSERT_ID();", plot);
    }

    public async Task<bool> UpdateAsync(Plot plot)
    {
        var args = new DynamicParameters(plot);
        args.Add("communityId", _user.CommunityId);

        // громаду ділянки не змінюємо
        using var conn = _db.CreateConnection();
        var rows = await conn.ExecuteAsync(
            $@"UPDATE plot p SET
              p.fullname=@FullName,
              p.village=@Village,
              p.street=@Street,
              p.housenumb=@HouseNumb,
              p.fieldnumber=@FieldNumber,
              p.plottype=@PlotType,
              p.plotnumber=@PlotNumber,
              p.plotarea=@PlotArea,
              p.cadastr=@Cadastr,
              p.tenant=@Tenant,
              p.url=@Url
              WHERE p.id=@Id AND {Scope}", args);
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.ExecuteAsync(
            $"DELETE p FROM plot p WHERE p.id = @id AND {Scope}",
            new { id, communityId = _user.CommunityId });
        return rows > 0;
    }

    public async Task<IEnumerable<Plot>> SearchAsync(
        string? fullName = null,
        string? village = null,
        string? street = null,
        string? houseNumb = null,
        string? fieldNumber = null,
        string? plotType = null,
        string? plotNumber = null,
        string? tenant = null,
        string? cadastr = null)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<Plot>(
            $@"{Select}
          WHERE {Scope}
            AND (@fullName IS NULL OR p.fullname LIKE CONCAT('%', @fullName, '%'))
            AND (@village IS NULL OR p.village LIKE CONCAT('%', @village, '%'))
            AND (@street IS NULL OR p.street LIKE CONCAT('%', @street, '%'))
            AND (@houseNumb IS NULL OR p.housenumb = @houseNumb)
            AND (@fieldNumber IS NULL OR p.fieldnumber LIKE CONCAT('%', @fieldNumber, '%'))
            AND (@plotType IS NULL OR p.plottype LIKE CONCAT('%', @plotType, '%'))
            AND (@plotNumber IS NULL OR p.plotnumber LIKE CONCAT('%', @plotNumber, '%'))
            AND (@tenant IS NULL OR p.tenant LIKE CONCAT('%', @tenant, '%'))
            AND (@cadastr IS NULL OR p.cadastr LIKE CONCAT('%', @cadastr, '%'))
          ORDER BY p.fullname",
            new
            {
                communityId = _user.CommunityId,
                fullName,
                village,
                street,
                houseNumb,
                fieldNumber,
                plotType,
                plotNumber,
                tenant,
                cadastr
            });
    }

    public async Task<bool> ExistsAsync(string? cadastr,
        string? village, string? street, string? houseNumb,
        string? plotType, int? excludeId = null)
    {
        using var conn = _db.CreateConnection();

        // дублікати шукаємо лише в межах громади
        var count = await conn.ExecuteScalarAsync<int>(
            $@"SELECT COUNT(*) FROM plot p
          WHERE (@excludeId IS NULL OR p.id != @excludeId)
          AND {Scope}
          AND (
              (@cadastr IS NOT NULL AND p.cadastr = @cadastr)
              OR (
                  LOWER(TRIM(@plotType)) = 'ожб'
                  AND LOWER(TRIM(p.village)) = LOWER(TRIM(@village))
                  AND LOWER(TRIM(p.street)) = LOWER(TRIM(@street))
                  AND LOWER(TRIM(p.housenumb)) = LOWER(TRIM(@houseNumb))
              )
          )",
            new { cadastr, village, street, houseNumb, plotType, excludeId, communityId = _user.CommunityId });

        return count > 0;
    }
}