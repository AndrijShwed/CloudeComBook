namespace CloudComBook.Shared.DTOs.Auth;

public record LoginRequest(
    string Login,
    string Password);
