using System.Threading.Tasks;
using AtcWeb.Domain;
using AtcWeb.Domain.GitHub;
using Ganss.XSS;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using Octokit;

namespace AtcWeb
{
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

            builder.Services.AddScoped<IGitHubClient>(_ =>
            {
                var tokenAuth = new Credentials(HttpClientConstants.AtcAccessToken);
                var gitHubClient = new GitHubClient(new ProductHeaderValue(HttpClientConstants.AtcOrganizationName))
                {
                    Credentials = tokenAuth,
                };

                return gitHubClient;
            });

            builder.Services.AddScoped<GitHubApiClient>();
            builder.Services.AddScoped<GitHubRepositoryService>();
            builder.Services.AddScoped<IHtmlSanitizer, HtmlSanitizer>(_ =>
            {
                var sanitizer = new HtmlSanitizer();
                sanitizer.AllowedAttributes.Add("class");
                return sanitizer;
            });

            builder.Services.AddMemoryCache();

            builder.Services.AddMudServices();

            return builder.Build().RunAsync();
        }
    }
}
