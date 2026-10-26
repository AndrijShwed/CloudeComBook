using System.Net.Http.Json;
using System.Security.Claims;
using CloudComBook.Shared.DTOs.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace CloudComBook.Web.Services;

public static class AccountEndpoints
{
    public static void MapAccountEndpoints(this WebApplication app)
    {
        app.MapPost("/Account/DoLogin", async (
            HttpContext context,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration) =>
        {
            var form = await context.Request.ReadFormAsync();
            var login = form["Login"].ToString();
            var password = form["Password"].ToString();
            var returnUrl = form["ReturnUrl"].ToString();

            // лише локальні адреси, щоб не було перенаправлення на сторонній сайт
            if (!IsLocalUrl(returnUrl))
                returnUrl = "/";

            var client = httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(configuration["ApiBaseUrl"] ?? "https://localhost:7079/");

            var response = await client.PostAsJsonAsync("api/auth/login",
                new LoginRequest(login, password));

            LoginResponse? result = null;
            if (response.IsSuccessStatusCode)
                result = await response.Content.ReadFromJsonAsync<LoginResponse>();

            if (result == null || !result.Success)
            {
                return Results.Redirect($"/Account/Login?error=1&returnUrl={Uri.EscapeDataString(returnUrl)}");
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, result.UserId.ToString()),
                new(ClaimTypes.Name, result.FullName ?? ""),
                new("Login", result.Login ?? ""),
                new("ShortName", result.ShortName ?? ""),
                new("AccessToken", result.Token ?? ""),
                new("CommunityId", result.CommunityId?.ToString() ?? "")
            };

            // супер-адмін має обидві ролі, тож AuthorizeView Roles="admin" працює й для нього
            if (result.Role == "superadmin")
            {
                claims.Add(new Claim(ClaimTypes.Role, "superadmin"));
                claims.Add(new Claim(ClaimTypes.Role, "admin"));
            }
            else
            {
                claims.Add(new Claim(ClaimTypes.Role, result.Role ?? ""));
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await context.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                });

            return Results.Redirect(returnUrl);
        });

        app.MapGet("/Account/Logout", async (HttpContext context) =>
        {
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Results.Redirect("/Account/Login");
        });
    }

    private static bool IsLocalUrl(string? url) =>
        !string.IsNullOrEmpty(url)
        && url[0] == '/'
        && (url.Length == 1 || (url[1] != '/' && url[1] != '\\'));
}