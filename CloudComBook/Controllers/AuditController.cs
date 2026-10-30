using CloudComBook.API.Data;
using CloudComBook.API.Services;
using CloudComBook.Shared.Models;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CloudComBook.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "admin")]   // супер-адмін теж має цю роль
public class AuditController : ControllerBase
{
    private readonly DbConnectionFactory _db;
    private readonly ICurrentUser _user;

    public AuditController(DbConnectionFactory db, ICurrentUser user)
    {
        _db = db;
        _user = user;
    }

    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] string? entity,
        [FromQuery] string? userLogin,
        [FromQuery] string? action,
        [FromQuery] int limit = 1000)
    {
        using var conn = _db.CreateConnection();

        var rows = await conn.QueryAsync<AuditLogEntry>(
            @"SELECT a.id AS Id, a.created_at AS CreatedAt, a.user_id AS UserId,
                     a.user_login AS UserLogin, a.community_id AS CommunityId,
                     c.name AS CommunityName,
                     a.entity AS Entity, a.entity_id AS EntityId, a.action AS Action,
                     a.old_values AS OldValues, a.new_values AS NewValues
              FROM audit_log a
              LEFT JOIN communities c ON c.id = a.community_id
              WHERE (@c IS NULL OR a.community_id = @c)
                AND (@from IS NULL OR a.created_at >= @from)
                AND (@to IS NULL OR a.created_at < DATE_ADD(@to, INTERVAL 1 DAY))
                AND (@entity IS NULL OR a.entity = @entity)
                AND (@userLogin IS NULL OR LOWER(a.user_login) LIKE CONCAT('%', LOWER(@userLogin), '%'))
                AND (@action IS NULL OR a.action = @action)
              ORDER BY a.id DESC
              LIMIT @limit",
            new
            {
                c = _user.CommunityId,
                from,
                to,
                entity,
                userLogin,
                action,
                limit = Math.Clamp(limit, 1, 2000)
            });

        return Ok(rows);
    }
}
