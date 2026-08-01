namespace ClaudeComBook.Desktop.Models;

public class UserInfo
{
    public int Id { get; set; }
    public string Login { get; set; } = "";
    public string Position { get; set; } = "";
    public string? FullName { get; set; }
    public string? ShortName { get; set; }
    public string Role { get; set; } = "user";
    public string Organization { get; set; } = "";
    public string Region { get; set; } = "";
    public string District { get; set; } = "";
    public string Village { get; set; } = "";
    public string Street { get; set; } = "";
    public string House { get; set; } = "";
    public string PostIndex { get; set; } = "";
    public string Phone { get; set; } = "";
}
