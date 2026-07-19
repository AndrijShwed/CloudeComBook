namespace ClaudeComBook.API.Models;

public class User
{
    public int Id { get; set; }
    public string Login { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string Role { get; set; } = "user";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}