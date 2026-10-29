using System.Net.Http.Headers;

namespace CloudComBook.Web.Services;

/// <summary>
/// Підставляє JWT-токен користувача (взятий з cookie-claims)
/// у заголовок Authorization кожного запиту до API.
/// </summary>
public class JwtAuthorizationHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public JwtAuthorizationHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override Task<HttpResponseMessage> SendAsync(
    HttpRequestMessage request,
    CancellationToken cancellationToken)
    {
        var ctx = _httpContextAccessor.HttpContext;

        var token = ctx?.User.Claims.FirstOrDefault(c => c.Type == "AccessToken")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        // обрана супер-адміном громада; API ігнорує цей заголовок для інших ролей
        if (ctx != null
            && ctx.User.IsInRole("superadmin")
            && int.TryParse(ctx.Request.Cookies["community_id"], out var communityId)
            && communityId > 0)
        {
            request.Headers.Remove("X-Community-Id");
            request.Headers.Add("X-Community-Id", communityId.ToString());
        }

        return base.SendAsync(request, cancellationToken);
    }
}
