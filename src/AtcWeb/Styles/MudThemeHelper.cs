using MudBlazor;
using MudBlazor.Utilities;

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
            Surface = Colors.Grey.Lighten3,
        };

        private static readonly Palette DarkPalette = new Palette
        {
            Black = "#27272F",
            White = Colors.Shades.White,
            Primary = "#AB74B5",
            PrimaryContrastText = Colors.Shades.White,
            Secondary = Colors.Grey.Lighten3,
            SecondaryContrastText = Colors.Shades.White,
            Tertiary = "#1EC8A5",
            TertiaryContrastText = Colors.Shades.White,
            Info = Colors.Blue.Default,
            InfoContrastText = Colors.Shades.White,
            Success = Colors.Green.Accent4,
            SuccessContrastText = Colors.Shades.White,
            Warning = Colors.Orange.Default,
            WarningContrastText = Colors.Shades.White,
            Error = Colors.Red.Default,
            ErrorContrastText = Colors.Shades.White,
            Dark = Colors.Grey.Darken3,
            DarkContrastText = Colors.Shades.White,
            TextPrimary = new MudColor(Colors.Shades.White).SetAlpha(0.50).ToString(MudColorOutputFormats.RGBA),
            TextSecondary = new MudColor(Colors.Shades.White).SetAlpha(0.40).ToString(MudColorOutputFormats.RGBA),
            TextDisabled = new MudColor(Colors.Shades.White).SetAlpha(0.20).ToString(MudColorOutputFormats.RGBA),
            ActionDefault = new MudColor(Colors.Shades.White).SetAlpha(0.40).ToString(MudColorOutputFormats.RGBA),
            ActionDisabled = new MudColor(Colors.Shades.White).SetAlpha(0.20).ToString(MudColorOutputFormats.RGBA),
            ActionDisabledBackground = new MudColor(Colors.Shades.White).SetAlpha(0.12).ToString(MudColorOutputFormats.RGBA),
            Background = "#32333D",
            BackgroundGrey = Colors.Grey.Lighten4,
            Surface = "#27272F",
            DrawerBackground = "#27272F",
            DrawerText = new MudColor(Colors.Shades.White).SetAlpha(0.50).ToString(MudColorOutputFormats.RGBA),
            DrawerIcon = Colors.Grey.Darken2,
            AppbarBackground = "#27272F",
            AppbarText = new MudColor(Colors.Shades.White).SetAlpha(0.70).ToString(MudColorOutputFormats.RGBA),
            LinesDefault = new MudColor(Colors.Shades.White).SetAlpha(0.12).ToString(MudColorOutputFormats.RGBA),
            LinesInputs = Colors.Grey.Lighten1,
            TableLines = new MudColor(Colors.Grey.Lighten2).SetAlpha(1.0).ToString(MudColorOutputFormats.RGBA),
            TableStriped = new MudColor(Colors.Shades.White).SetAlpha(0.02).ToString(MudColorOutputFormats.RGBA),
            TableHover = new MudColor(Colors.Shades.White).SetAlpha(0.04).ToString(MudColorOutputFormats.RGBA),
            Divider = Colors.Grey.Lighten2,
            DividerLight = new MudColor(Colors.Shades.White).SetAlpha(0.8).ToString(MudColorOutputFormats.RGBA),

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