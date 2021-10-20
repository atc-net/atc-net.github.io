using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.Extensions.Caching.Memory;

// ReSharper disable StringLiteralTypo
namespace AtcWeb.Domain.GitHub
{
    public class GitHubHtmlClient
    {
        private readonly HttpClient httpClient;
        private readonly IMemoryCache memoryCache;

        [SuppressMessage("Minor Code Smell", "S1075:URIs should not be hardcoded", Justification = "OK.")]
        public GitHubHtmlClient(HttpClient httpClient, IMemoryCache memoryCache)
        {
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            this.memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));

            this.httpClient = new HttpClient(); //// TODO: Remove when Introduce typed httpclient..
            this.httpClient.BaseAddress = new Uri("https://github.com"); //// TODO: Introduce typed httpclient..
            this.httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Linux; Android 6.0; Nexus 5 Build/MRA58N) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.106 Mobile Safari/537.36");
            this.httpClient.DefaultRequestHeaders.Add("Access-Control-Allow-Origin", "*");
            this.httpClient.DefaultRequestHeaders.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
            //// this.httpClient.DefaultRequestHeaders.Add("mode", "no-cors");
        }

        public async Task<(bool isSuccessful, HtmlDocument)> GetHtmlAtcCode(string repositoryName, CancellationToken cancellationToken)
        {
            var url = $"/atc-net/{repositoryName}";
            var cacheKey = $"{CacheConstants.CacheKeyCodeFile}_{url}";
            if (memoryCache.TryGetValue(cacheKey, out HtmlDocument data))
            {
                return (true, data);
            }

            try
            {
                Console.WriteLine($"#LP-1 - {repositoryName} - {url}"); // TODO: Remove-debug

                var result = await httpClient.GetStringAsync(new Uri(url), cancellationToken);

                Console.WriteLine($"#LP-2 - {repositoryName}"); // TODO: Remove-debug

                if (string.IsNullOrEmpty(result))
                {
                    return (false, new HtmlDocument());
                }

                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(result);

                Console.WriteLine($"#LP-3 - {repositoryName}"); // TODO: Remove-debug

                memoryCache.Set(cacheKey, htmlDoc);
                return (true, htmlDoc);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"#LP-Exception - {repositoryName} - {ex.Message}"); // TODO: Remove-debug
                return (false, new HtmlDocument());
            }
        }
    }
}