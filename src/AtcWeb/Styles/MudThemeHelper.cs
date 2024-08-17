namespace AtcWeb.Styles;

public static class MudThemeHelper
{
    public static readonly PaletteLight LightPalette = new()
    {
        Primary = "#6EAEDF",
        Surface = Colors.Gray.Lighten3,
        AppbarBackground = "#27272F",
    };

    public static readonly PaletteDark DarkPalette = new()
    {
        Primary = "#6EAEDF",
    };

    public static readonly LayoutProperties LayoutProperties = new()
    {
        DrawerWidthLeft = "300px"
    };

    public static MudTheme Create()
        => new()
        {
            PaletteLight = LightPalette,
            PaletteDark = DarkPalette,
            LayoutProperties = LayoutProperties,
        };
}