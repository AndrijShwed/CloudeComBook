using CloudComBook.API.Repositories.Interfaces;
using CloudComBook.API.Services;
using CloudComBook.Shared.DTOs.Auth;
using CloudComBook.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CloudComBook.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AuthController : ControllerBase
{
    private static readonly string[] AssignableRoles = { "reader", "user", "admin" };

    private readonly IUserRepository _repo;
    private readonly JwtService _jwt;
    private readonly ICurrentUser _current;
    private readonly IAuditService _audit;
    private readonly ICommunityRepository _communities;

    public AuthController(IUserRepository repo, JwtService jwt, ICurrentUser current, IAuditService audit)
    {
        _repo = repo;
        _jwt = jwt;
        _current = current;
        _audit = audit;
    }

    // ---------- допоміжні ----------

    /// Супер-адмін керує будь-ким; адмін лише користувачами своєї громади, крім супер-адмінів.
    private bool CanManage(User target) =>
        _current.IsSuperAdmin ||
        (target.CommunityId == _current.CommunityId && target.Role != "superadmin");

    private bool CanAccessProfile(User target) =>
        target.Id == _current.UserId || (User.IsInRole("admin") && CanManage(target));

    private bool RoleAllowed(string? role) =>
        role == "superadmin" ? _current.IsSuperAdmin : AssignableRoles.Contains(role);

    // у журнал без PasswordHash
    private static object Safe(User u) =>
        new { u.Id, u.Login, u.FullName, u.Role, u.Position, u.CommunityId, u.IsActive };

    private static UserProfileResponse ToProfile(User user) => new()
    {
        Id = user.Id,
        Login = user.Login,
        FullName = user.FullName,
        Role = user.Role,
        Position = user.Position,
        Organization = user.Organization,
        Region = user.Region,
        District = user.District,
        Village = user.Village,
        Street = user.Street,
        House = user.House,
        ShortName = user.ShortName,
        Phone = user.Phone,
        PostIndex = user.PostIndex,
        IsActive = user.IsActive,
        CommunityId = user.CommunityId,
    };

    // ---------- вхід ----------

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _repo.GetByLoginAsync(request.Login);
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Unauthorized(new LoginResponse { Success = false, Message = "Невірний логін або пароль" });

        var token = _jwt.GenerateToken(user);

        return Ok(new LoginResponse
        {
            Success = true,
            Token = token,
            UserId = user.Id,
            Login = user.Login,
            FullName = user.FullName,
            ShortName = user.ShortName,
            Role = user.Role,
            CommunityId = user.CommunityId
        });
    }

    // ---------- керування користувачами ----------

    [Authorize(Roles = "admin")]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var role = request.Role ?? "user";
        if (!RoleAllowed(role))
            return Forbid();

        int? communityId;
        if (role == "superadmin")
        {
            communityId = null;
        }
        else if (_current.IsSuperAdmin)
        {
            // супер-адмін: громада з форми, інакше та, що вибрана перемикачем
            communityId = request.CommunityId ?? (_current.CommunityId is > 0 ? _current.CommunityId : null);
            if (communityId == null)
                return BadRequest(new { message = "Оберіть громаду." });
            if (await _communities.GetByIdAsync(communityId.Value) == null)
                return BadRequest(new { message = "Громаду не знайдено." });
        }
        else if (_current.CommunityId is int c and > 0)
        {
            communityId = c; // адмін громади: завжди своя, значення з форми ігнорується
        }
        else
        {
            return BadRequest(new { message = "Оберіть громаду." });
        }

        var existing = await _repo.GetByLoginAsync(request.Login);
        if (existing != null)
            return Conflict(new { message = "Користувач вже існує" });

        var user = new User
        {
            Login = request.Login ?? "",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FullName = request.FullName ?? "",
            Role = role,
            Position = request.Position ?? "",
            CommunityId = communityId
        };
        user.Id = await _repo.CreateAsync(user);

        await _audit.LogAsync("users", user.Id, "CREATE", null, Safe(user));
        return Ok(new { user.Id, user.Login, user.FullName, user.Role, user.Position });
    }

    [Authorize(Roles = "admin")]
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        // адмін: лише своя громада; супер-адмін: обрана громада або всі
        var users = await _repo.GetAllAsync(_current.CommunityId);
        return Ok(users.Select(ToProfile));
    }

    [Authorize(Roles = "admin")]
    [HttpPut("users/{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request)
    {
        var user = await _repo.GetByIdAsync(id);
        if (user == null || !CanManage(user)) return NotFound();

        var newRole = request.Role ?? user.Role;
        if (newRole != user.Role && !RoleAllowed(newRole))
            return Forbid();

        var old = Safe(user);

        user.Login = request.Login ?? "";
        if (!string.IsNullOrEmpty(request.Password))
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        user.FullName = request.FullName ?? "";
        user.Role = newRole;
        user.Position = request.Position ?? "";
        user.Region = request.Region ?? "";
        user.District = request.District ?? "";
        user.Village = request.Village ?? "";
        user.Street = request.Street ?? "";
        user.House = request.House ?? "";
        user.ShortName = request.ShortName ?? "";
        user.Organization = request.Organization ?? "";
        user.Phone = request.Phone ?? "";
        user.PostIndex = request.PostIndex ?? "";
        user.IsActive = request.IsActive;

        if (newRole == "superadmin")
            user.CommunityId = null;
        else if (_current.IsSuperAdmin && request.CommunityId.HasValue)
            user.CommunityId = request.CommunityId; // змінювати громаду може лише супер-адмін

        if (!await _repo.UpdateAsync(user)) return BadRequest();

        await _audit.LogAsync("users", id, "UPDATE", old, Safe(user));
        return NoContent();
    }

    [Authorize(Roles = "admin")]
    [HttpPut("users/{id}/toggle")]
    public async Task<IActionResult> ToggleActive(int id, [FromBody] bool isActive)
    {
        var user = await _repo.GetByIdAsync(id);
        if (user == null || !CanManage(user)) return NotFound();

        if (!await _repo.SetActiveAsync(id, isActive)) return NotFound();

        await _audit.LogAsync("users", id, "UPDATE",
            new { user.Login, user.IsActive }, new { user.Login, IsActive = isActive });
        return NoContent();
    }

    [Authorize(Roles = "admin")]
    [HttpDelete("users/{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _repo.GetByIdAsync(id);
        if (user == null || !CanManage(user)) return NotFound();

        if (user.Role is "admin" or "superadmin")
            return BadRequest(new { message = "Видалення користувачів з роллю admin заборонено" });

        if (!await _repo.DeleteAsync(id)) return NotFound();

        await _audit.LogAsync("users", id, "DELETE", Safe(user), null);
        return NoContent();
    }

    // ---------- власний профіль ----------

    [Authorize(Roles = "reader,user,admin")]
    [HttpGet("users/{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var user = await _repo.GetByIdAsync(id);
        if (user == null || !CanAccessProfile(user)) return NotFound();

        return Ok(ToProfile(user));
    }

    [Authorize(Roles = "reader,user,admin")]
    [HttpPut("users/{id}/settings")]
    public async Task<IActionResult> UpdateSettings(int id, [FromBody] UpdateSettingsRequest request)
    {
        var user = await _repo.GetByIdAsync(id);
        if (user == null || !CanAccessProfile(user)) return NotFound();

        var old = Safe(user);

        user.FullName = request.FullName ?? "";
        user.Position = request.Position ?? "";
        user.Region = request.Region ?? "";
        user.District = request.District ?? "";
        user.Village = request.Village ?? "";
        user.Street = request.Street ?? "";
        user.House = request.House ?? "";
        user.ShortName = request.ShortName ?? "";
        user.Organization = request.Organization ?? "";
        user.Phone = request.Phone ?? "";
        user.PostIndex = request.PostIndex ?? "";

        if (!await _repo.UpdateAsync(user)) return BadRequest();

        await _audit.LogAsync("users", id, "UPDATE", old, Safe(user));
        return NoContent();
    }
}