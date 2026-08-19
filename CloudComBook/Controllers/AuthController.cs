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
    private readonly IUserRepository _repo;

    private readonly JwtService _jwt;

    public AuthController(IUserRepository repo, JwtService jwt)
    {
        _repo = repo;
        _jwt = jwt;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _repo.GetByLoginAsync(request.Login);
        if (user == null)
            return Unauthorized(new LoginResponse { Success = false, Message = "Невірний логін або пароль" });

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
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
            Role = user.Role
        });
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var existing = await _repo.GetByLoginAsync(request.Login);
        if (existing != null)
            return Conflict(new { message = "Користувач вже існує" });

        var user = new User
        {
            Login = request.Login ?? "",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FullName = request.FullName ?? "",
            Role = request.Role ?? "user",
            Position = request.Position ?? ""
        };
        user.Id = await _repo.CreateAsync(user);
        return Ok(new { user.Id, user.Login, user.FullName, user.Role, user.Position });
    }

    [Authorize(Roles = "admin")]
    [HttpDelete("users/{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _repo.GetByIdAsync(id);
        if (user == null) return NotFound();

        if (user.Role == "admin")
            return BadRequest(new { message = "Видалення користувачів з роллю admin заборонено" });

        var ok = await _repo.DeleteAsync(id);
        return ok ? NoContent() : NotFound();
    }

    [Authorize(Roles = "admin")]
    [HttpPut("users/{id}/toggle")]
    public async Task<IActionResult> ToggleActive(int id, [FromBody] bool isActive)
    {
        var ok = await _repo.SetActiveAsync(id, isActive);
        return ok ? NoContent() : NotFound();
    }

    [Authorize(Roles = "admin")]
    [HttpPut("users/{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request)
    {
        var user = await _repo.GetByIdAsync(id);
        if (user == null) return NotFound();

        user.Login = request.Login ?? "";
        if (!string.IsNullOrEmpty(request.Password))
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        user.FullName = request.FullName ?? "";
        user.Role = request.Role ?? "";
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

        var ok = await _repo.UpdateAsync(user);
        return ok ? NoContent() : BadRequest();
    }

    [Authorize(Roles = "admin")]
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _repo.GetAllAsync();

        var result = users.Select(user => new UserProfileResponse
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
            IsActive = user.IsActive
        });

        return Ok(result);
    }

    [Authorize(Roles = "reader,user,admin")]
    [HttpGet("users/{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var user = await _repo.GetByIdAsync(id);
        if (user == null) return NotFound();

        return Ok(new UserProfileResponse
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
            IsActive = user.IsActive
        });
    }

    [Authorize(Roles = "reader,user,admin")]
    [HttpPut("users/{id}/settings")]
    public async Task<IActionResult> UpdateSettings(int id, [FromBody] UpdateSettingsRequest request)
    {
        var user = await _repo.GetByIdAsync(id);
        if (user == null) return NotFound();

        user.FullName = request.FullName ?? "";
        user.Position = request.Position ?? "";
        user.Region = request.Region?? "";
        user.District = request.District?? "";
        user.Village = request.Village ?? "";
        user.Street = request.Street ?? "";
        user.House = request.House ?? "";
        user.ShortName = request.ShortName ?? "";
        user.Organization = request.Organization ?? "";
        user.Phone = request.Phone ?? "";
        user.PostIndex = request.PostIndex ?? "";

        var ok = await _repo.UpdateAsync(user);
        return ok ? NoContent() : BadRequest();
    }
}
