namespace ClaudeComBook.Desktop.Services;

public class UserInfo
{
    public int Id { get; set; }
    public string Login { get; set; } = "";
    public string? FullName { get; set; }
    public string? ShortName { get; set; }
    public string Role { get; set; } = "user";
    public string Position { get; set; } = "";
    public string? Organization { get; set; } = "";
    public string? Region { get; set; } = "";
    public string? District { get; set; } = "";
    public string? Phone { get; set; } = "";
    public string? House { get; set; } = "";
    public string? PostIndex { get; set; } = "";
    public string? Village { get; set; } = "";
    public string? Street { get; set; } = "";
}

public static class AppSession
{
    public static UserInfo? CurrentUser { get; set; }

    public static bool IsAdmin => CurrentUser?.Role == "admin";
    public static bool IsUser => CurrentUser?.Role == "user" || IsAdmin;
    public static bool IsReader => CurrentUser?.Role == "reader" || IsUser;
    public static string Position => CurrentUser?.Position ?? "";
    public static string Organization => CurrentUser?.Organization ?? "";
    public static string Region => CurrentUser?.Region ?? "";
    public static string District => CurrentUser?.District ?? "";
    public static string Phone => CurrentUser?.Phone ?? "";
    public static string Village => CurrentUser?.Village ?? "";
    public static string Street => CurrentUser?.Street ?? "";
    public static string House => CurrentUser?.House ?? "";
    public static string PostIndex => CurrentUser?.PostIndex ?? "";
    public static string ShortName => CurrentUser?.ShortName ?? "";
}
