// AuditService.cs
using System.Text.Json;
using System.Text.Json.Serialization;
using CloudComBook.API.Data;
using Dapper;

namespace CloudComBook.API.Services;

public interface IAuditService
{
    Task LogAsync(string entity, int? entityId, string action, object? oldValues, object? newValues);
}

public class AuditService : IAuditService
{
    private static readonly JsonSerializerOptions Json = new()
    {
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };

    private readonly DbConnectionFactory _db;
    private readonly ICurrentUser _user;
    private readonly IHttpContextAccessor _http;
    private readonly ILogger<AuditService> _log;

    public AuditService(DbConnectionFactory db, ICurrentUser user,
        IHttpContextAccessor http, ILogger<AuditService> log)
    {
        _db = db;
        _user = user;
        _http = http;
        _log = log;
    }

    public async Task LogAsync(string entity, int? entityId, string action, object? oldValues, object? newValues)
    {
        try
        {
            using var conn = _db.CreateConnection();
            await conn.ExecuteAsync(
                @"INSERT INTO audit_log
                    (user_id, user_login, community_id, entity, entity_id, action, old_values, new_values, ip_address)
                  VALUES (@uid, @login, @cid, @entity, @eid, @action, @old, @new, @ip)",
                new
                {
                    uid = _user.UserId,
                    login = _user.Login,
                    cid = _user.CommunityId is > 0 ? _user.CommunityId : null,
                    entity,
                    eid = entityId,
                    action,
                    old = oldValues == null ? null : JsonSerializer.Serialize(oldValues, Json),
                    @new = newValues == null ? null : JsonSerializer.Serialize(newValues, Json),
                    ip = _http.HttpContext?.Connection.RemoteIpAddress?.ToString()
                });
        }
        catch (Exception ex)
        {
            // зміна вже збережена, тож відповідь не ламаємо, але збій журналу має бути видно в логах
            _log.LogError(ex, "Не вдалося записати аудит {Entity}/{Id}", entity, entityId);
        }
    }
}