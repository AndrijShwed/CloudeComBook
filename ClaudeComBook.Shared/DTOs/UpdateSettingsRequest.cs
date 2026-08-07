namespace ClaudeComBook.Shared.DTOs;

public record UpdateSettingsRequest(
    string? FullName,
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
