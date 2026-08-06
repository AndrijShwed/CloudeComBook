using MudBlazor;

namespace ClaudeComBook.Web.Theme;

public static class AppTheme
{
    public static MudTheme Default = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = Colors.Blue.Default,
            Secondary = Colors.Green.Default,
            Background = "#f5f5f5",
            Surface = Colors.Shades.White,
            AppbarBackground = Colors.Blue.Darken2,
            DrawerBackground = Colors.Shades.White,
            DrawerText = Colors.Gray.Darken3
        },

        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "10px"
        }
    };
}