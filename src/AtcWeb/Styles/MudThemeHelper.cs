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
        Primary = "#776be7",
        Secondary = "#ff4081",
        Tertiary = "#1ec8a5",
        Info = "#3299ff",
        Success = "#0bba83",
        Warning = "#ffa800",
        Error = "#f64e62",
        Black = "#27272f",
        Background = "#1a1a27",
        BackgroundGray = "#151521",
        Surface = "#1e1e2d",
        DrawerBackground = "#1a1a27",
        DrawerText = "#92929f",
        DrawerIcon = "#92929f",
        AppbarBackground = "#262638",
        AppbarText = "#c8c6d6",
        TextPrimary = "#b2b0bf",
        TextSecondary = "#92929f",
        TextDisabled = "rgba(255,255,255,0.2)",
        ActionDefault = "#74718e",
        ActionDisabled = "rgba(153,153,153,0.3)",
        ActionDisabledBackground = "rgba(96,95,109,0.3)",
        Divider = "#292838",
        DividerLight = "rgba(255,255,255,0.06)",
        TableLines = "#33323e",
        LinesDefault = "#33323e",
        LinesInputs = "rgba(255,255,255,0.3)",
        GrayLight = "#2a2833",
        GrayLighter = "#1e1e2d",
        OverlayLight = "#1e1e2d80",
    };

    public static readonly LayoutProperties LayoutProperties = new()
    {
        DrawerWidthLeft = "300px",
    };

    public static MudTheme Create()
        => new()
        {
            PaletteLight = LightPalette,
            PaletteDark = DarkPalette,
            LayoutProperties = LayoutProperties,
        };
}