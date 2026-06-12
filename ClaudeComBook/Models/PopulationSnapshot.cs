namespace ClaudeComBook.API.Models;

public class PopulationSnapshot
{
    public int Id { get; set; }
    public string? SettlementName { get; set; }
    public int? Year { get; set; }
    public int? Population { get; set; }
    public DateTime? CreatedAt { get; set; }
}