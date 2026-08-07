namespace ClaudeComBook.Shared.DTOs;

public class LoginResponse
{
    public bool Success { get; set; }

    public string Message { get; set; } = string.Empty;

    public string Token { get; set; } = string.Empty;

    public int UserId { get; set; }

    public string Login { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string ShortName { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;
}
