using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Microsoft.Extensions.Caching.Memory;

// ReSharper disable StringLiteralTypo
namespace AtcWeb.Domain.GitHub.Clients
{
    public class GitHubHtmlClient
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IMemoryCache memoryCache;

        public GitHubHtmlClient(IHttpClientFactory httpClientFactory, IMemoryCache memoryCache)
        {
            this.httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            this.memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        }

        public async Task<(bool isSuccessful, HtmlDocument)> GetHtmlAtcCode(string repositoryName, CancellationToken cancellationToken)
        {
            var url = $"/atc-net/{repositoryName}";
            var cacheKey = $"{CacheConstants.CacheKeyCodeFile}_{url}";
            if (memoryCache.TryGetValue(cacheKey, out HtmlDocument data))
            {
                return (isSuccessful: true, data);
            }

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.SetBrowserRequestMode(BrowserRequestMode.NoCors);

                Console.WriteLine($"#LP-1 - {repositoryName} - {url}"); // TODO: Remove-debug
                var httpClient = httpClientFactory.CreateClient(HttpClientConstants.GitHubHtmlClient);
                var response = await httpClient.SendAsync(request, cancellationToken);

                var result = await response.Content.ReadAsStringAsync(cancellationToken);
                Console.WriteLine($"StatusCode: {response.StatusCode}");
                Console.WriteLine(result);

                Console.WriteLine($"#LP-2 - {repositoryName}"); // TODO: Remove-debug

                if (string.IsNullOrEmpty(result))
                {
                    return (isSuccessful: false, new HtmlDocument());
                }

                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(result);

                Console.WriteLine($"#LP-3 - {repositoryName}"); // TODO: Remove-debug

                memoryCache.Set(cacheKey, htmlDoc);
                return (isSuccessful: true, htmlDoc);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"#LP-Exception - {repositoryName} - {ex.Message}"); // TODO: Remove-debug
                return (false, new HtmlDocument());
            }
        }
    }
}