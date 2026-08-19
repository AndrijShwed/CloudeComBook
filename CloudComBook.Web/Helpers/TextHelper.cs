namespace CloudComBook.Web.Helpers;

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

    public static string? CapitalizeWords(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return value;

        var chars = value.ToCharArray();
        bool capitalizeNext = true;

        for (int i = 0; i < chars.Length; i++)
        {
            if (char.IsWhiteSpace(chars[i]) || chars[i] == '-' || chars[i] == '\'')
            {
                capitalizeNext = true;
                continue;
            }

            chars[i] = capitalizeNext ? char.ToUpper(chars[i]) : char.ToLower(chars[i]);
            capitalizeNext = false;
        }

        return new string(chars);
    }

    public static string? FormatCadastr(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return value;

        // Залишаємо тільки цифри
        var digits = new string(value.Where(char.IsDigit).ToArray());

        // Обрізаємо зайве (максимум 10+2+3+4 = 19 цифр)
        if (digits.Length > 19)
            digits = digits.Substring(0, 19);

        var groups = new[] { 10, 2, 3, 4 };
        var result = new System.Text.StringBuilder();
        int pos = 0;

        foreach (var groupLen in groups)
        {
            if (pos >= digits.Length) break;

            if (result.Length > 0)
                result.Append(':');

            var len = Math.Min(groupLen, digits.Length - pos);
            result.Append(digits.Substring(pos, len));
            pos += len;
        }

        return result.ToString();
    }
}
