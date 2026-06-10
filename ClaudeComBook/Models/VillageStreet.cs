namespace ClaudeComBook.API.Models
{
    public class VillageStreet
    {
        public int Id { get; set; }
        public int VillageId { get; set; }
        public int StreetId { get; set; }
        public int? OldStreetId { get; set; }
        public bool IsActive { get; set; }
        public DateTime? RenameDate { get; set; }
        public byte[]? FileData { get; set; }
    }
}
