using System.Net.Http.Json;
using System.Security.Claims;
using CloudeComBook.Shared.DTOs.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace CloudeComBook.Web.Services;

public static class AccountEndpoints
{
    public static void MapAccountEndpoints(this WebApplication app)
    {
        app.MapPost("/Account/DoLogin", async (
            HttpContext context,
            IHttpClientFactory httpClientFactory) =>
        {
            var form = await context.Request.ReadFormAsync();
            var login = form["Login"].ToString();
            var password = form["Password"].ToString();
            var returnUrl = form["ReturnUrl"].ToString();

            var client = httpClientFactory.CreateClient();
            client.BaseAddress = new Uri("https://localhost:7079/");

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
                new(ClaimTypes.Role, result.Role ?? ""),
                new("Login", result.Login ?? ""),
                new("ShortName", result.ShortName ?? ""),
                new("AccessToken", result.Token ?? "")
            };

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

            return Results.Redirect(string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl);
        });

        app.MapGet("/Account/Logout", async (HttpContext context) =>
        {
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Results.Redirect("/Account/Login");
        });
    }
}