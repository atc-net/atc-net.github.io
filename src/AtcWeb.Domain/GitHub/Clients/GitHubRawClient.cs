using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

// ReSharper disable StringLiteralTypo
namespace AtcWeb.Domain.GitHub.Clients
{
    public class GitHubRawClient
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IMemoryCache memoryCache;

        public GitHubRawClient(IHttpClientFactory httpClientFactory, IMemoryCache memoryCache)
        {
            this.httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            this.memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        }

        public async Task<(bool isSuccessful, string)> GetRawAtcCodeFile(string repositoryName, string defaultBranchName, string filePath, CancellationToken cancellationToken)
        {
            try
            {
                var url = $"/atc-net/{repositoryName}/{defaultBranchName}/{filePath}";
                var cacheKey = $"{CacheConstants.CacheKeyCodeFile}_{url}";
                if (memoryCache.TryGetValue(cacheKey, out string data))
                {
                    return (isSuccessful: true, data);
                }

                var httpClient = httpClientFactory.CreateClient(HttpClientConstants.GitHubRawClient);
                var result = await httpClient.GetStringAsync(url, cancellationToken);
                if (string.IsNullOrEmpty(result))
                {
                    return (isSuccessful: false, string.Empty);
                }

                memoryCache.Set(cacheKey, result, CacheConstants.AbsoluteExpirationRelativeToNow);
                return (isSuccessful: true, result);
            }
            catch (Exception ex)
            {
                return (isSuccessful: false, ex.Message);
            }
        }
    }
}