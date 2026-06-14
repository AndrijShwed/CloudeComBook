// Models/VillageStreet.cs
namespace ClaudeComBook.Desktop.Models;

public class VillageStreet
{
    public int Id { get; set; }
    public int VillageId { get; set; }
    public int StreetId { get; set; }
    public bool IsActive { get; set; }
}
