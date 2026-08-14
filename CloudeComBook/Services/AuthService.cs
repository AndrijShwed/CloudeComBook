using BCrypt.Net;
using CloudeComBook.API.Repositories.Interfaces;
using CloudeComBook.Shared.DTOs.Auth;
using CloudeComBook.Shared.Models;

namespace CloudeComBook.API.Services;

public class AuthService
{
    private readonly IUserRepository _userRepository;
    private readonly JwtService _jwtService;

    public AuthService(
        IUserRepository userRepository,
        JwtService jwtService)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByLoginAsync(request.Login);

        if (user == null)
        {
            return new LoginResponse
            {
                Success = false,
                Message = "Неправильний логін або пароль."
            };
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return new LoginResponse
            {
                Success = false,
                Message = "Неправильний логін або пароль."
            };
        }

        var token = _jwtService.GenerateToken(user);

        return new LoginResponse
        {
            Success = true,
            Token = token,
            UserId = user.Id,
            Login = user.Login,
            FullName = user.FullName,
            ShortName = user.ShortName,
            Role = user.Role
        };
    }
}