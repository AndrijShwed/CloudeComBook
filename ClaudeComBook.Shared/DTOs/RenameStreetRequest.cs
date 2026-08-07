namespace ClaudeComBook.Shared.DTOs.Streets;

public record RenameStreetRequest(
    int VillageId,
    int OldStreetId,
    int NewStreetId,
    DateTime? RenameDate,
    string? FileData);
