namespace ClaudeComBook.API.Models;

public class Enterprise
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Owner { get; set; }
    public int? EmployeesNumber { get; set; }
    public int? VillageStreetId { get; set; }
    public string? HouseNumber { get; set; }
}
