using ClaudeComBook.API.Data;
using ClaudeComBook.API.Models;
using ClaudeComBook.API.Repositories.Interfaces;
using Dapper;
using System.Text.Json;

namespace ClaudeComBook.API.Repositories;

public class PlotRepository : IPlotRepository
{
    private readonly DbConnectionFactory _db;

    public PlotRepository(DbConnectionFactory db) => _db = db;

    public async Task<IEnumerable<Plot>> GetAllAsync()
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<Plot>(
            "SELECT * FROM plot ORDER BY village");
    }

    public async Task<Plot?> GetByIdAsync(int id)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Plot>(
            "SELECT * FROM plot WHERE id = @id", new { id });
    }

    public async Task<IEnumerable<Plot>> SearchAsync(string query)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<Plot>(
            @"SELECT * FROM plot 
              WHERE village LIKE @q
              OR street LIKE @q
              OR fullname LIKE @q
              OR cadastr LIKE @q
              OR tenant LIKE @q
              ORDER BY village
              LIMIT 50",
            new { q = $"%{query}%" });
    }

    public async Task<int> CreateAsync(Plot plot)
    {
            using var conn = _db.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            @"INSERT INTO plot 
              (fullname, village, street, housenumb, fieldnumber, plottype, plotnumber,
               plotarea, cadastr, tenant, url)
              VALUES
              (@FullName, @Village, @Street, @HouseNumb, @FieldNumber, @PlotType, @PlotNumber,
               @PlotArea, @Cadastr, @Tenant, @Url);
              SELECT LAST_INSERT_ID();", plot);

    }

    public async Task<bool> UpdateAsync(Plot plot)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.ExecuteAsync(
            @"UPDATE plot SET
              fullname=@Fullname,
              village=@Village,
              street=@Street,
              housenumb=@Housenumb,
              fieldnumber=@Fieldnumber,
              plottype=@Plottype,
              plotnumber=@Plotnumber,
              plotarea=@Plotarea,
              cadastr=@Cadastr,
              tenant=@Tenant,
              url=@Url
              WHERE id=@Id", plot);
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.ExecuteAsync(
            "DELETE FROM plot WHERE id = @id", new { id });
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
            @"SELECT * FROM plot
          WHERE
            (@fullName IS NULL OR fullname LIKE CONCAT('%', @fullName, '%'))
            AND (@village IS NULL OR village LIKE CONCAT('%', @village, '%'))
            AND (@street IS NULL OR street LIKE CONCAT('%', @street, '%'))
            AND (@houseNumb IS NULL OR housenumb = @houseNumb)
            AND (@fieldNumber IS NULL OR fieldnumber LIKE CONCAT('%', @fieldNumber, '%'))
            AND (@plotType IS NULL OR plottype LIKE CONCAT('%', @plotType, '%'))
            AND (@plotNumber IS NULL OR plotnumber LIKE CONCAT('%', @plotNumber, '%'))
            AND (@tenant IS NULL OR tenant LIKE CONCAT('%', @tenant, '%'))
            AND (@cadastr IS NULL OR cadastr LIKE CONCAT('%', @cadastr, '%'))
          ORDER BY fullname",
            new
            {
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
}

