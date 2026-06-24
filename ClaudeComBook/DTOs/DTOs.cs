namespace ClaudeComBook.API.DTOs
{
    public class DTOs
    {
        public record UpdateFileRequest(string FileData);

        public record RenameStreetRequest(
                        int VillageId,
                        int OldStreetId,
                        int NewStreetId,
                        DateTime? RenameDate,
                        string? FileData);
    }
}
