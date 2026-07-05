namespace ClaudeComBook.Desktop.Models;

public class House
{
    public int IdHouses { get; set; }
    public int VillageStreetId { get; set; }
    public string? NumbOfHouse { get; set; }
    public string? LastName { get; set; }
    public string? Name { get; set; }
    public string? Surname { get; set; }
    public decimal? TotalArea { get; set; }
    public decimal? LivingArea { get; set; }
    public int? TotalOfRooms { get; set; }
}
