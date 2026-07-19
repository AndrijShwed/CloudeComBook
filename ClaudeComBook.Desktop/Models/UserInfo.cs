namespace ClaudeComBook.Desktop.Models;

public class UserInfo
{
    public int Id { get; set; }
    public string Login { get; set; } = "";
    public string? FullName { get; set; }
    public string Role { get; set; } = "user";
}
