// Models/VillageStreet.cs
using System;

namespace ClaudeComBook.Desktop.Models;

public class VillageStreet
{
    public int Id { get; set; }
    public int VillageId { get; set; }
    public int StreetId { get; set; }
    public bool IsActive { get; set; }
    public string? VillageName { get; set; }
    public string? StreetName { get; set; }
    public string? OldStreetName { get; set; }
    public DateTime? RenameDate { get; set; }
    public bool HasFile => FileData != null && FileData.Length > 0;
    public byte[]? FileData { get; set; }
}
