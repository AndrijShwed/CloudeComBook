namespace ClaudeComBook.API.Models;

public class Plot
{
    public int Id { get; set; }
    public string? FullName { get; set; }
    public string? Village { get; set; }
    public string? Street { get; set; }
    public string? HouseNumb { get; set; }
    public string? FieldNumber { get; set; }
    public string? PlotType { get; set; }
    public string? PlotNumber { get; set; }
    public decimal? PlotArea { get; set; }
    public string? Cadastr { get; set; }
    public string? Tenant { get; set; }
    public string? Url { get; set; }
}
