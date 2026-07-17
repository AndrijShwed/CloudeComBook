using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public sealed class HouseNumberComparer : IComparer<string>
{
    private static readonly Regex Regex = new(
        @"^(?<main>\d+)" +
        @"(?:/(?<sub>\d+))?" +
        @"(?:[-\s]?(?<suffix>[^\d].*))?$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public int Compare(string? x, string? y)
    {
        x = Normalize(x);
        y = Normalize(y);

        var mx = Regex.Match(x);
        var my = Regex.Match(y);

        if (!mx.Success || !my.Success)
            return StringComparer.CurrentCultureIgnoreCase.Compare(x, y);

        int result = CompareInt(mx.Groups["main"], my.Groups["main"]);
        if (result != 0) return result;

        result = CompareInt(mx.Groups["sub"], my.Groups["sub"]);
        if (result != 0) return result;

        return StringComparer.CurrentCultureIgnoreCase.Compare(
            mx.Groups["suffix"].Value,
            my.Groups["suffix"].Value);
    }

    private static int CompareInt(Group a, Group b)
    {
        bool ha = a.Success;
        bool hb = b.Success;

        if (!ha && !hb) return 0;
        if (!ha) return -1;
        if (!hb) return 1;

        return int.Parse(a.Value).CompareTo(int.Parse(b.Value));
    }

    private static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "";

        return value.Trim()
                    .Replace(" ", "")
                    .Replace("–", "-")
                    .ToUpperInvariant();
    }
}