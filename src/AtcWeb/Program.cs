namespace AtcWeb;

public static class Program
{
    public static Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");

        builder.Services.AddScoped(_ => new DefaultBrowserOptionsMessageHandler
        {
            DefaultBrowserRequestCache = BrowserRequestCache.NoStore,
            DefaultBrowserRequestMode = BrowserRequestMode.NoCors,
        });

        builder.Services.AddScoped<AtcApiGitHubApiInformationClient>();
        builder.Services.AddScoped<AtcApiGitHubRepositoryClient>();
        builder.Services.AddScoped<GitHubRepositoryService>();
        builder.Services.AddScoped<IHtmlSanitizer, HtmlSanitizer>(_ =>
        {
            var sanitizer = new HtmlSanitizer();
            sanitizer.AllowedAttributes.Add("class");
            return sanitizer;
        });

        builder.Services.AddSingleton<StateContainer>();

        builder.Services.AddMemoryCache();

        builder.Services.AddMudServices();

        builder.Services.AddColorThemePreferenceDetector();

        return builder.Build().RunAsync();
    }
}