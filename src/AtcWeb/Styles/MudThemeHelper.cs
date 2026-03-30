namespace AtcWeb.Styles;

public static class MudThemeHelper
{
    public static readonly PaletteLight LightPalette = new()
    {
        Primary = "#6C5CE7",
        Secondary = "#E84393",
        Tertiary = "#00B894",
        Info = "#3282F6",
        Success = "#0BBA83",
        Warning = "#F59E0B",
        Error = "#EF4444",
        Black = "#1a1a2e",
        Background = "#F8F7FC",
        BackgroundGray = "#F0EEF6",
        Surface = "#FFFFFF",
        DrawerBackground = "#F8F7FC",
        DrawerText = "#4A4568",
        DrawerIcon = "#6C5CE7",
        AppbarBackground = "#1a1a2e",
        AppbarText = "#E2E0EF",
        TextPrimary = "#2D2B42",
        TextSecondary = "#6B6988",
        ActionDefault = "#6B6988",
        Divider = "#E2E0EF",
        DividerLight = "rgba(108,92,231,0.08)",
        TableLines = "#E2E0EF",
        LinesDefault = "#E2E0EF",
        LinesInputs = "#C4C1D9",
        GrayLight = "#F0EEF6",
        GrayLighter = "#F8F7FC",
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
        Black = "#0f0f1a",
        Background = "#0f0f1a",
        BackgroundGray = "#0a0a14",
        Surface = "#16162a",
        DrawerBackground = "#0f0f1a",
        DrawerText = "#9896ab",
        DrawerIcon = "#776be7",
        AppbarBackground = "#16162ae6",
        AppbarText = "#d0cee0",
        TextPrimary = "#c8c6d8",
        TextSecondary = "#9896ab",
        TextDisabled = "rgba(255,255,255,0.2)",
        ActionDefault = "#74718e",
        ActionDisabled = "rgba(153,153,153,0.3)",
        ActionDisabledBackground = "rgba(96,95,109,0.3)",
        Divider = "#1e1e36",
        DividerLight = "rgba(255,255,255,0.06)",
        TableLines = "#1e1e36",
        LinesDefault = "#1e1e36",
        LinesInputs = "rgba(255,255,255,0.3)",
        GrayLight = "#1a1a30",
        GrayLighter = "#16162a",
        OverlayLight = "#0f0f1a80",
    };

    public static readonly LayoutProperties LayoutProperties = new()
    {
        DrawerWidthLeft = "280px",
    };

    public static MudTheme Create()
        => new()
        {
            PaletteLight = LightPalette,
            PaletteDark = DarkPalette,
            LayoutProperties = LayoutProperties,
        };
}