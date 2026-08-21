//using CloudComBook.Web.Components;
//using CloudComBook.Web.Services;
//using CloudComBook.Web.ViewModels;
//using Microsoft.AspNetCore.Authentication.Cookies;
//using MudBlazor.Services;

//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.
//builder.Services.AddRazorComponents()
//    .AddInteractiveServerComponents();
//builder.Services.AddMudServices();

//builder.Services.AddHttpContextAccessor();
//builder.Services.AddCascadingAuthenticationState();

//// ---- Cookie Authentication ----
//builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
//    .AddCookie(options =>
//    {
//        options.LoginPath = "/Account/Login";
//        options.LogoutPath = "/Account/Logout";
//        options.AccessDeniedPath = "/Account/AccessDenied";
//        options.ExpireTimeSpan = TimeSpan.FromHours(8);
//        options.SlidingExpiration = true;
//        options.Cookie.Name = "CloudComBook.Auth";
//    });

//builder.Services.AddAuthorization();
//// ---- кінець блоку автентифікації ----

//// Handler, що підставляє JWT з cookie-claims у заголовок Authorization
//builder.Services.AddTransient<JwtAuthorizationHandler>();

//builder.Services.AddHttpClient<ApiService>(client =>
//{
//    client.BaseAddress = new Uri("https://localhost:7079/");
//})
//.AddHttpMessageHandler<JwtAuthorizationHandler>();

//builder.Services.AddScoped<PeopleViewModel>();

//var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (!app.Environment.IsDevelopment())
//{
//    app.UseExceptionHandler("/Error");
//    app.UseHsts();
//}
//app.UseStatusCodePagesWithReExecute("/not-found");
//app.UseHttpsRedirection();

//app.UseAuthentication();   // ВАЖЛИВО: перед UseAuthorization()
//app.UseAuthorization();

//app.UseAntiforgery();

//app.UseStaticFiles();
//app.MapRazorComponents<App>()
//    .AddInteractiveServerRenderMode();

//app.MapAccountEndpoints();  // мінімальні API для логіну/логауту

//app.Run();



using CloudComBook.Web.Components;
using CloudComBook.Web.Services;
using CloudComBook.Web.ViewModels;
using Microsoft.AspNetCore.Authentication.Cookies;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddMudServices();

builder.Services.AddHttpContextAccessor();
builder.Services.AddCascadingAuthenticationState();

// ---- Cookie Authentication ----
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.Name = "CloudComBook.Auth";
    });

builder.Services.AddAuthorization();
// ---- кінець блоку автентифікації ----

// Handler, що підставляє JWT з cookie-claims у заголовок Authorization
builder.Services.AddTransient<JwtAuthorizationHandler>();

builder.Services.AddHttpClient<ApiService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7079/");
})
.AddHttpMessageHandler<JwtAuthorizationHandler>();

builder.Services.AddScoped<PeopleViewModel>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found");
app.UseHttpsRedirection();

// Статичні файли (JS/CSS для сторінок, MudBlazor) обслуговуються до авторизації.
app.UseStaticFiles();

app.UseAuthentication();   // ВАЖЛИВО: перед UseAuthorization()
app.UseAuthorization();

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapAccountEndpoints();  // мінімальні API для логіну/логауту

app.Run();



