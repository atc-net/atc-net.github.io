using System.Threading.Tasks;
using Atc.Blazor.ColorThemePreference;
using AtcWeb.State;
using AtcWeb.Styles;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AtcWeb.Shared
{
    public partial class MainLayout : LayoutComponentBase
    {
        private bool drawerOpen;
        private NavMenu navMenuRef;

        [Inject]
        private StateContainer StateContainer { get; set; }

        [Inject]
        private IColorThemePreferenceDetector ColorThemePreferenceDetector { get; set; }

        [Inject]
        private NavigationManager NavigationManager { get; set; }

        private void DrawerToggle()
        {
            drawerOpen = !drawerOpen;
        }

        protected override async Task OnInitializedAsync()
        {
            var useDarkMode = await ColorThemePreferenceDetector.UseDarkMode();
            StateContainer.CurrentTheme = useDarkMode
                ? MudThemeHelper.DarkTheme
                : MudThemeHelper.LightTheme;

            drawerOpen = true;
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
            StateContainer.CurrentTheme = StateContainer.CurrentTheme == MudThemeHelper.LightTheme
                ? MudThemeHelper.DarkTheme
                : MudThemeHelper.LightTheme;
        }
    }
}