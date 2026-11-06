namespace AtcWeb.Helpers;

public static class AtcAppFilter
{
    /// <summary>
    /// The side menu's app filter: matches name, display name or description, ignoring case.
    /// </summary>
    /// <param name="app">The app.</param>
    /// <param name="filter">The filter text; empty matches every app.</param>
    /// <returns><see langword="true"/> when the app matches.</returns>
    public static bool Matches(
        AtcApp app,
        string? filter)
    {
        ArgumentNullException.ThrowIfNull(app);

        if (string.IsNullOrWhiteSpace(filter))
        {
            return true;
        }

        var term = filter.Trim();
        return app.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
               app.DisplayName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
               (app.Description?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false);
    }
}