namespace AtcWeb.Styles;

public static class MudThemeHelper
{
    private static readonly Typography AtcTypography = new()
    {
        Default = new Default
        {
            // Use MudBlazor default fonts
        },
    };

    private static readonly LayoutProperties AtcLayoutProperties = new()
    {
        DrawerWidthLeft = "320px",
    };

    private static readonly PaletteLight LightPalette = new()
    {
        Primary = "#2A91C4",
        Surface = Colors.Grey.Lighten3,
        AppbarBackground = "#27272F",
    };

    private static readonly PaletteDark DarkPalette = new()
    {
        Primary = "#6EAEDF",
    };

    public static readonly MudTheme LightTheme = new()
    {
        Palette = LightPalette,
        LayoutProperties = AtcLayoutProperties,
        Typography = AtcTypography,
    };

    public static readonly MudTheme DarkTheme = new()
    {
        Palette = DarkPalette,
        LayoutProperties = AtcLayoutProperties,
        Typography = AtcTypography,
    };
}