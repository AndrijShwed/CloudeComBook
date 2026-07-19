namespace ClaudeComBook.Desktop.Services;

public class UserInfo
{
    public int Id { get; set; }
    public string Login { get; set; } = "";
    public string? FullName { get; set; }
    public string Role { get; set; } = "user";
}

public static class AppSession
{
    public static UserInfo? CurrentUser { get; set; }

    public static bool IsAdmin => CurrentUser?.Role == "admin";
    public static bool IsUser => CurrentUser?.Role == "user" || IsAdmin;
    public static bool IsReader => CurrentUser?.Role == "reader" || IsUser;
}
