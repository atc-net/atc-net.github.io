using System;
using System.Net.Http;
using System.Threading.Tasks;
using AtcWeb.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;

namespace AtcWeb
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.AddScoped<GitHubApiClient>();

            builder.Services.AddMemoryCache();

            builder.Services.AddMudServices();

            await builder.Build().RunAsync();
        }
    }
}