using System;
using System.Net.Http;
using System.Threading.Tasks;
using AtcWeb.Domain.GitHub;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;

namespace AtcWeb
{
    public static class Program
    {
        public static Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.AddScoped<GitHubApiClient>();
            builder.Services.AddScoped<GitHubRepositoryService>();

            builder.Services.AddMemoryCache();

            builder.Services.AddMudServices();

            return builder.Build().RunAsync();
        }
    }
}