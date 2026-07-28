using ClaudeComBook.API.Models;
using ClaudeComBook.API.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ClaudeComBook.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _repo;

    public AuthController(IUserRepository repo) => _repo = repo;

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _repo.GetByLoginAsync(request.Login);
        if (user == null) return Unauthorized(new { message = "Невірний логін або пароль" });
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Unauthorized(new { message = "Невірний логін або пароль" });

        return Ok(new
        {
            user.Id,
            user.Login,
            user.FullName,
            user.Role,
            user.Position
        });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var existing = await _repo.GetByLoginAsync(request.Login);
        if (existing != null)
            return Conflict(new { message = "Користувач вже існує" });

        var user = new User
        {
            Login = request.Login,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FullName = request.FullName,
            Role = request.Role ?? "user",
            Position = request.Position
        };
        user.Id = await _repo.CreateAsync(user);
        return Ok(new { user.Id, user.Login, user.FullName, user.Role, user.Position });
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers() =>
        Ok(await _repo.GetAllAsync());

    [HttpDelete("users/{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var ok = await _repo.DeleteAsync(id);
        return ok ? NoContent() : NotFound();
    }

    [HttpPut("users/{id}/toggle")]
    public async Task<IActionResult> ToggleActive(int id, [FromBody] bool isActive)
    {
        var ok = await _repo.SetActiveAsync(id, isActive);
        return ok ? NoContent() : NotFound();
    }

    [HttpPut("users/{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request)
    {
        var user = await _repo.GetByIdAsync(id);
        if (user == null) return NotFound();

        user.Login = request.Login;
        if (!string.IsNullOrEmpty(request.Password))
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        user.FullName = request.FullName;
        user.Role = request.Role;
        user.Position = request.Position;

        var ok = await _repo.UpdateAsync(user);
        return ok ? NoContent() : BadRequest();
    }

    [HttpGet("users/{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var user = await _repo.GetByIdAsync(id);
        if (user == null) return NotFound();
        return Ok(new { user.Id, user.Login, user.FullName, user.Role, user.Position });
    }

    [HttpPut("users/{id}/settings")]
    public async Task<IActionResult> UpdateSettings(int id, [FromBody] UpdateSettingsRequest request)
    {
        var user = await _repo.GetByIdAsync(id);
        if (user == null) return NotFound();

        user.FullName = request.FullName;
        user.Position = request.Position;

        var ok = await _repo.UpdateAsync(user);
        return ok ? NoContent() : BadRequest();
    }

    
}

public record LoginRequest(string Login, string Password);
public record RegisterRequest(string Login, string Password, string? FullName, string? Role, string? Position);
public record UpdateUserRequest(string Login, string? Password, string? FullName, string Role, string? Position);
public record UpdateSettingsRequest(string? FullName, string? Position);