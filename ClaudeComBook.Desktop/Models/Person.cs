using System;

namespace ClaudeComBook.Desktop.Models;

public class Person
{
    public int PeopleId { get; set; }
    public string? LastName { get; set; }
    public string? Name { get; set; }
    public string? Surname { get; set; }
    public string? Sex { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? NumbOfHouse { get; set; }
    public string? Passport { get; set; }
    public string? Registr { get; set; }
    public string? IdKod { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Status { get; set; }
    public string? MilID { get; set; }
    public int? VillageStreetId { get; set; }
    public string? VillageName { get; set; }
    public string? StreetName { get; set; }
    public DateTime? MDate { get; set; }
    public string? Description { get; set; }
    public string MDateFormatted =>
    MDate.HasValue && MDate.Value.Year > 1
        ? MDate.Value.ToString("dd.MM.yyyy")
        : "";
}
