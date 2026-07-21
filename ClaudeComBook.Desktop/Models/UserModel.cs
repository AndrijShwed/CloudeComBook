using System;

namespace ClaudeComBook.Desktop.Models;

public class UserModel
{
    public int Id { get; set; }
    public string Login { get; set; } = "";
    public string? FullName { get; set; }
    public string Role { get; set; } = "user";
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public string IsActiveText => IsActive ? "Так" : "Ні";
}
