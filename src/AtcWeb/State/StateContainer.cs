namespace AtcWeb.State;

public class StateContainer
{
    private MudTheme currentTheme = MudThemeHelper.DarkTheme;

    public event Action<object, EventArgs>? OnThemeChange;

    public MudTheme CurrentTheme
    {
        get => currentTheme;
        set
        {
            if (value == currentTheme)
            {
                return;
            }

            currentTheme = value;
            NotifyThemeStateChanged();
        }
    }

    public bool IsFireAndForgetTriggerStarted { get; set; }

    public void NotifyThemeStateChanged() => OnThemeChange?.Invoke(this, EventArgs.Empty);
}