namespace AtcWeb.State;

public class StateContainer
{
    private bool isDarkMode;

    public event Action<object, EventArgs>? OnThemeChange;

    public MudTheme Theme { get; } = MudThemeHelper.Create();

    public bool IsDarkMode
    {
        get => isDarkMode;
        set
        {
            if (isDarkMode == value)
            {
                return;
            }

            isDarkMode = value;
            NotifyThemeStateChanged();
        }
    }

    public void DarkModeToggle()
        => IsDarkMode = !IsDarkMode;

    public void UseDarkMode(bool useDarkMode)
        => IsDarkMode = useDarkMode;

    public string SuccessColor
        => IsDarkMode
            ? Theme.PaletteDark.Success.Value
            : Theme.PaletteLight.Success.Value;

    public string WarningColor => IsDarkMode
        ? Theme.PaletteDark.Warning.Value
        : Theme.PaletteLight.Warning.Value;

    public string ErrorColor => IsDarkMode
        ? Theme.PaletteDark.Error.Value
        : Theme.PaletteLight.Error.Value;

    public string DarkLightModeIcon
        => isDarkMode switch
        {
            true => Icons.Material.Rounded.LightMode,
            false => Icons.Material.Outlined.DarkMode,
        };

    public void NotifyThemeStateChanged()
        => OnThemeChange?.Invoke(this, EventArgs.Empty);
}