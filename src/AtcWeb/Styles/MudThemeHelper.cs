using MudBlazor;

namespace AtcWeb.Styles
{
    public static class MudThemeHelper
    {
        private static readonly Typography AtcTypography = new Typography
        {
            Default = new Default
            {
                // Use MudBlazor default fonts
            },
        };

        private static readonly LayoutProperties AtcLayoutProperties = new LayoutProperties
        {
            DrawerWidthLeft = "320px",
        };

        private static readonly Palette LightPalette = new Palette
        {
            // Use MudBlazor default colors
        };

        private static readonly Palette DarkPalette = new Palette
        {
            // TODO: Align with Colors.cs... Material.io
            Primary = Colors.DeepPurple.Accent2,
            Secondary = Colors.Grey.Lighten3,
            Black = "#27272F",
            Background = "#32333D",
            BackgroundGrey = "#32333d",
            Surface = "#373740",
            DrawerBackground = "#27272F",
            DrawerText = "rgba(255,255,255, 0.50)",
            DrawerIcon = "rgba(255,255,255, 0.50)",
            AppbarBackground = "#27272F",
            AppbarText = "rgba(255,255,255, 0.70)",
            TextPrimary = "rgba(255,255,255, 0.70)",
            TextSecondary = "rgba(255,255,255, 0.50)",
            ActionDefault = "#adadb1",
            ActionDisabled = "rgba(255,255,255, 0.26)",
            ActionDisabledBackground = "rgba(255,255,255, 0.12)",
            Divider = "rgba(255,255,255, 0.12)",
            DividerLight = "rgba(255,255,255, 0.06)",
            TableLines = "rgba(255,255,255, 0.12)",
            LinesDefault = "rgba(255,255,255, 0.12)",
            LinesInputs = "rgba(255,255,255, 0.3)",
            TextDisabled = "rgba(255,255,255, 0.2)",
            PrimaryContrastText = "#ff0000",
            Info = "#3299FF",
            Success = "#0BBA83",
            Warning = "#FFA800",
            Error = "#F64E62",
            Dark = "#27272F",
        };

        public static readonly MudTheme LightTheme = new MudTheme
        {
            Palette = LightPalette,
            LayoutProperties = AtcLayoutProperties,
            Typography = AtcTypography,
        };

        public static readonly MudTheme DarkTheme = new MudTheme
        {
            Palette = DarkPalette,
            LayoutProperties = AtcLayoutProperties,
            Typography = AtcTypography,
        };
    }
}