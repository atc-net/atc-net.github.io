using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Atc.Serialization;
using Microsoft.Extensions.Caching.Memory;

// ReSharper disable StringLiteralTypo
namespace AtcWeb.Domain.GitHub
{
    public class GitHubRawClient
    {
        private readonly HttpClient httpClient;
        private readonly IMemoryCache memoryCache;

        [SuppressMessage("Minor Code Smell", "S1075:URIs should not be hardcoded", Justification = "OK.")]
        public GitHubRawClient(HttpClient httpClient, IMemoryCache memoryCache)
        {
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            this.memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));

            this.httpClient = new HttpClient(); //// TODO: Remove when Introduce typed httpclient..
            this.httpClient.BaseAddress = new Uri("https://raw.githubusercontent.com"); //// TODO: Introduce typed httpclient..
            this.httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Linux; Android 6.0; Nexus 5 Build/MRA58N) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.106 Mobile Safari/537.36");
        }

        public async Task<(bool isSuccessful, string)> GetRawAtcCodeFile(string repositoryName, string defaultBranchName, string filePath, CancellationToken cancellationToken)
        {
            var url = $"/atc-net/{repositoryName}/{defaultBranchName}/{filePath}";
            var cacheKey = $"{CacheConstants.CacheKeyCodeFile}_{url}";
            if (memoryCache.TryGetValue(cacheKey, out string data))
            {
                return (true, data);
            }

            try
            {
                var result = await httpClient.GetStringAsync(new Uri(url), cancellationToken);
                if (string.IsNullOrEmpty(result))
                {
                    return (false, string.Empty);
                }

                memoryCache.Set(cacheKey, result);
                return (true, result);
            }
            catch
            {
                return (false, string.Empty);
            }
        }
    }
}