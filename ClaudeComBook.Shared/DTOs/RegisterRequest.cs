namespace ClaudeComBook.Shared.DTOs;

public record RegisterRequest(
    string Login,
    string Password,
    string? FullName,
    string? Role,
    string? Position);
