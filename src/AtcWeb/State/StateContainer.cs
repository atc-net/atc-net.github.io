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
            isDarkMode = value;
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

    public void UseDarkMode(
        bool useDarkMode)
        => IsDarkMode = !IsDarkMode;

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

    public void NotifyThemeStateChanged()
        => OnThemeChange?.Invoke(this, EventArgs.Empty);
}