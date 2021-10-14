using AtcWeb.Extensions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AtcWeb.Shared
{
    public partial class MainLayout : LayoutComponentBase
    {
        private bool drawerOpen = false;
        private NavMenu navMenuRef;

        [Inject] private NavigationManager NavigationManager { get; set; }

        private void DrawerToggle()
        {
            drawerOpen = !drawerOpen;
        }

        protected override void OnInitialized()
        {
            currentTheme = defaultTheme;

            // If not home page, the navbar starts open
            if (!NavigationManager.IsHomePage())
            {
                drawerOpen = true;
            }
        }

        protected override void OnAfterRender(bool firstRender)
        {
            navMenuRef.Refresh();
        }

        private void OnSwipe(SwipeDirection direction)
        {
            switch (direction)
            {
                case SwipeDirection.LeftToRight when !drawerOpen:
                    drawerOpen = true;
                    StateHasChanged();
                    break;
                case SwipeDirection.RightToLeft when drawerOpen:
                    drawerOpen = false;
                    StateHasChanged();
                    break;
            }
        }

        private void DarkMode()
        {
            currentTheme = currentTheme == defaultTheme
                ? darkTheme
                : defaultTheme;
        }

        private MudTheme currentTheme = new();

        private readonly MudTheme defaultTheme = new()
        {
            Palette = new Palette
            {
                Black = "#272c34"
            }
        };

        private readonly MudTheme darkTheme = new()
        {
            Palette = new Palette
            {
                Primary = "#776be7",
                Black = "#27272f",
                Background = "#32333d",
                BackgroundGrey = "#27272f",
                Surface = "#373740",
                DrawerBackground = "#27272f",
                DrawerText = "rgba(255,255,255, 0.50)",
                DrawerIcon = "rgba(255,255,255, 0.50)",
                AppbarBackground = "#27272f",
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
                Info = "#3299ff",
                Success = "#0bba83",
                Warning = "#ffa800",
                Error = "#f64e62",
                Dark = "#27272f"
            }
        };
    }
}