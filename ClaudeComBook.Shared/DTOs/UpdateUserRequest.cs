namespace ClaudeComBook.Shared.DTOs;

public record UpdateUserRequest(
    string Login,
    string? Password,
    string? FullName,
    string Role,
    string? Position,
    string? Region,
    string? District,
    string? Village,
    string? Street,
    string? House,
    string? ShortName,
    string? Organization,
    string? Phone,
    string? PostIndex);
