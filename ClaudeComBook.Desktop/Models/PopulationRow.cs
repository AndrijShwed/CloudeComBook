using System.Collections.Generic;

namespace ClaudeComBook.Desktop.Models;

public class PopulationRow
{
    public int Year { get; set; }
    public Dictionary<string, int> VillagePopulations { get; set; } = new();
    public int Total { get; set; }
    public bool IsCurrentYear => Year == System.DateTime.Now.Year;
}