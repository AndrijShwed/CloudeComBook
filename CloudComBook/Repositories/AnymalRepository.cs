using CloudComBook.API.Data;
using CloudComBook.API.Repositories.Interfaces;
using CloudComBook.API.Services;
using CloudComBook.Shared.DTOs;
using CloudComBook.Shared.Models;
using Dapper;

namespace CloudComBook.API.Repositories;

public class AnymalRepository : IAnymalRepository
{
    // @communityId = NULL означає «усі громади» (лише супер-адмін без вибраної громади)
    private const string Scope = "(@communityId IS NULL OR a.community_id = @communityId)";

    private const string Select = @"
        SELECT a.anymalsId AS AnymalsId, a.lastname AS LastName, a.name AS Name,
               a.surname AS Surname, a.village AS Village, a.anymals AS Anymals,
               a.covs AS Covs, a.pigs AS Pigs, a.sheeps AS Sheeps, a.goats AS Goats,
               a.horses AS Horses, a.birds AS Birds, a.rabbits AS Rabbits,
               a.beeses AS Beeses, a.community_id AS CommunityId
        FROM anymals a";

    private readonly DbConnectionFactory _db;
    private readonly ICurrentUser _user;

    public AnymalRepository(DbConnectionFactory db, ICurrentUser user)
    {
        _db = db;
        _user = user;
    }

    public async Task<IEnumerable<Anymal>> GetAllAsync()
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<Anymal>(
            $"{Select} WHERE {Scope} ORDER BY a.lastname, a.name",
            new { communityId = _user.CommunityId });
    }

    public async Task<Anymal?> GetByIdAsync(int id)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Anymal>(
            $"{Select} WHERE a.anymalsId = @id AND {Scope}",
            new { id, communityId = _user.CommunityId });
    }

    public async Task<IEnumerable<Anymal>> SearchAsync(string query)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<Anymal>(
            $@"{Select}
              WHERE (a.lastname LIKE @q OR a.name LIKE @q OR a.village LIKE @q)
                AND {Scope}
              ORDER BY a.lastname, a.name
              LIMIT 50",
            new { q = $"%{query}%", communityId = _user.CommunityId });
    }

    public async Task<int> CreateAsync(Anymal anymal)
    {
        // CommunityId виставляє контролер із токена, значенню з клієнта не довіряємо
        using var conn = _db.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            @"INSERT INTO anymals
              (lastname, name, surname, village, anymals, covs, pigs,
               sheeps, goats, horses, birds, rabbits, beeses, community_id)
              VALUES
              (@LastName, @Name, @Surname, @Village, @Anymals, @Covs, @Pigs,
               @Sheeps, @Goats, @Horses, @Birds, @Rabbits, @Beeses, @CommunityId);
              SELECT LAST_INSERT_ID();", anymal);
    }

    public async Task<bool> UpdateAsync(Anymal anymal)
    {
        var args = new DynamicParameters(anymal);
        args.Add("communityId", _user.CommunityId);

        // громаду запису не змінюємо
        using var conn = _db.CreateConnection();
        var rows = await conn.ExecuteAsync(
            $@"UPDATE anymals a SET
              a.lastname=@LastName, a.name=@Name, a.surname=@Surname,
              a.village=@Village, a.anymals=@Anymals, a.covs=@Covs,
              a.pigs=@Pigs, a.sheeps=@Sheeps, a.goats=@Goats,
              a.horses=@Horses, a.birds=@Birds, a.rabbits=@Rabbits,
              a.beeses=@Beeses
              WHERE a.anymalsId=@AnymalsId AND {Scope}", args);
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.ExecuteAsync(
            $"DELETE a FROM anymals a WHERE a.anymalsId = @id AND {Scope}",
            new { id, communityId = _user.CommunityId });
        return rows > 0;
    }

    public async Task<bool> ExistsAsync(string lastName, string name, string? surname, string village)
    {
        using var conn = _db.CreateConnection();
        var count = await conn.ExecuteScalarAsync<int>(
            $@"SELECT COUNT(*) FROM anymals a
          WHERE a.lastname = @lastName
          AND a.name = @name
          AND a.village = @village
          AND (@surname IS NULL OR a.surname = @surname)
          AND {Scope}",
            new { lastName, name, surname, village, communityId = _user.CommunityId });
        return count > 0;
    }

    public async Task<IEnumerable<Anymal>> SearchAsync(
        string? lastName = null,
        string? name = null,
        string? surname = null,
        string? village = null,
        bool hasCovs = false,
        bool hasHorses = false,
        bool hasPigs = false,
        bool hasSheeps = false,
        bool hasGoats = false,
        bool hasBirds = false,
        bool hasRabbits = false,
        bool hasBeeses = false)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<Anymal>(
            $@"{Select}
      WHERE {Scope}
        AND (@lastName IS NULL OR LOWER(a.lastname) LIKE CONCAT('%', LOWER(@lastName), '%'))
        AND (@name IS NULL OR LOWER(a.name) LIKE CONCAT('%', LOWER(@name), '%'))
        AND (@surname IS NULL OR LOWER(a.surname) LIKE CONCAT('%', LOWER(@surname), '%'))
        AND (@village IS NULL OR a.village = @village)
        AND (@hasCovs = 0 OR a.covs > 0)
        AND (@hasHorses = 0 OR a.horses > 0)
        AND (@hasPigs = 0 OR a.pigs > 0)
        AND (@hasSheeps = 0 OR a.sheeps > 0)
        AND (@hasGoats = 0 OR a.goats > 0)
        AND (@hasBirds = 0 OR a.birds > 0)
        AND (@hasRabbits = 0 OR a.rabbits > 0)
        AND (@hasBeeses = 0 OR a.beeses > 0)
      ORDER BY a.lastname, a.name",
            new
            {
                communityId = _user.CommunityId,
                lastName,
                name,
                surname,
                village,
                hasCovs,
                hasHorses,
                hasPigs,
                hasSheeps,
                hasGoats,
                hasBirds,
                hasRabbits,
                hasBeeses
            });
    }

    public async Task<IEnumerable<AnymalVillageStatisticsDto>> GetStatisticsByVillageAsync()
    {
        using var conn = _db.CreateConnection();
        // групуємо ще й за громадою, щоб однойменні села різних громад не зливалися
        return await conn.QueryAsync<AnymalVillageStatisticsDto>(
            $@"SELECT
            a.village AS Village,
            SUM(a.anymals) + SUM(a.covs) AS Vrh,
            SUM(a.covs) AS Covs,
            SUM(a.pigs) AS Pigs,
            SUM(a.sheeps) AS Sheeps,
            SUM(a.goats) AS Goats,
            SUM(a.horses) AS Horses,
            SUM(a.birds) AS Birds,
            SUM(a.rabbits) AS Rabbits,
            SUM(a.beeses) AS Beeses
          FROM anymals a
          WHERE {Scope}
          GROUP BY a.community_id, a.village
          ORDER BY a.village",
            new { communityId = _user.CommunityId });
    }
}