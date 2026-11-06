var builder = WebAssemblyHostBuilder.CreateDefault(args);

// The Aspire AppHost passes the API address as a WebAssembly environment variable (see AtcWeb.csproj);
// otherwise it comes from wwwroot/appsettings*.json.
AtcApiConstants.Configure(
    Environment.GetEnvironmentVariable("AtcApi__BaseAddress")
    ?? builder.Configuration["AtcApi:BaseAddress"]);

builder.RootComponents.Add<App>("#app");

builder.Services.AddScoped(_ => new HttpClient());
builder.Services.AddScoped<BrowserCacheService>();

builder.Services.AddScoped<AtcApiGitHubApiInformationClient>();
builder.Services.AddScoped<AtcApiGitHubRepositoryClient>();
builder.Services.AddScoped<AtcApiGitHubWikiClient>();
builder.Services.AddScoped<AtcApiNugetClient>();
builder.Services.AddScoped<GitHubRepositoryService>();
builder.Services.AddScoped<AtcApiAppClient>();
builder.Services.AddScoped<AtcAppService>();
builder.Services.AddScoped<IHtmlSanitizer, HtmlSanitizer>(_ =>
{
    var sanitizer = new HtmlSanitizer();
    sanitizer.AllowedAttributes.Add("class");
    sanitizer.AllowedAttributes.Add("id");
    return sanitizer;
});

builder.Services.AddSingleton<StateContainer>();

builder.Services.AddMemoryCache();

builder.Services.AddMudServices();

await builder.Build().RunAsync();