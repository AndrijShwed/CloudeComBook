namespace CloudeComBook.Shared.DTOs.Auth;

public record RegisterRequest(
    string Login,
    string Password,
    string? FullName,
    string? Role,
    string? Position);
