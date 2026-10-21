// CurrentUser.cs
using System.Security.Claims;

namespace CloudComBook.API.Services;

public interface ICurrentUser
{
    int UserId { get; }
    string Login { get; }
    bool IsSuperAdmin { get; }
    /// null означає «усі громади»: лише супер-адмін без вибраної громади
    int? CommunityId { get; }
    int RequireCommunity();
}

public class CurrentUser : ICurrentUser
{
    private readonly HttpContext? _ctx;
    private readonly ClaimsPrincipal _p;

    public CurrentUser(IHttpContextAccessor accessor)
    {
        _ctx = accessor.HttpContext;
        _p = _ctx?.User ?? new ClaimsPrincipal();
    }

    public int UserId => int.TryParse(
        _p.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? _p.FindFirst("sub")?.Value, out var id) ? id : 0;

    public string Login => _p.Identity?.Name ?? "";

    public bool IsSuperAdmin => _p.IsInRole("superadmin");

    public int? CommunityId
    {
        get
        {
            if (IsSuperAdmin)
                return int.TryParse(_ctx?.Request.Headers["X-Community-Id"], out var c) ? c : null;

            // звичайний користувач: лише громада з токена; без неї -1, що не збігається ні з чим
            return int.TryParse(_p.FindFirst("community_id")?.Value, out var cid) ? cid : -1;
        }
    }

    public int RequireCommunity() =>
        CommunityId ?? throw new InvalidOperationException("Оберіть громаду.");
}