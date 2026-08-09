using System.Net.Http.Headers;

namespace ClaudeComBook.Web.Services;

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
        var token = _httpContextAccessor.HttpContext?.User
            .Claims.FirstOrDefault(c => c.Type == "AccessToken")?.Value;

        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
