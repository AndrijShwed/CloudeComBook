namespace ClaudeComBook.Web.Helpers;

public static class TextHelper
{
    public static string? CapitalizeFirst(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return value;

        value = value.TrimStart();
        if (value.Length == 0)
            return value;

        return char.ToUpper(value[0]) + value.Substring(1);
    }
}
