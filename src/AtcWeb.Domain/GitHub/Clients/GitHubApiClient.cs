using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Atc.Serialization;
using AtcWeb.Domain.GitHub.Models;
using Microsoft.Extensions.Caching.Memory;

// ReSharper disable StringLiteralTypo
namespace AtcWeb.Domain.GitHub.Clients
{
    public class GitHubApiClient
    {
        private static readonly SemaphoreSlim LockObject = new SemaphoreSlim(1, 1);
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IMemoryCache memoryCache;
        private readonly JsonSerializerOptions jsonSerializerOptions;

        public GitHubApiClient(IHttpClientFactory httpClientFactory, IMemoryCache memoryCache)
        {
            this.httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            this.memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            this.jsonSerializerOptions = JsonSerializerOptionsFactory.Create();
        }

        public async Task<(bool isSuccessful, List<GitHubRepository>)> GetAtcRepositories(CancellationToken cancellationToken)
        {
            if (memoryCache.TryGetValue(CacheConstants.CacheKeyRepositories, out List<GitHubRepository> data))
            {
                return (isSuccessful: true, data);
            }

            try
            {
                var httpClient = httpClientFactory.CreateClient(HttpClientConstants.GitHubApiClient);
                var result = await httpClient.GetFromJsonAsync<List<GitHubRepository>>(
                    "/orgs/atc-net/repos",
                    jsonSerializerOptions,
                    cancellationToken);

                if (result is null)
                {
                    return (isSuccessful: false, new List<GitHubRepository>());
                }

                if (result.Count > 0)
                {
                    memoryCache.Set(CacheConstants.CacheKeyRepositories, result);
                }

                return (isSuccessful: true, result);
            }
            catch
            {
                return (isSuccessful: false, new List<GitHubRepository>());
            }
        }

        public async Task<(bool isSuccessful, List<GitHubContributor>)> GetAtcContributors(CancellationToken cancellationToken)
        {
            await LockObject.WaitAsync(cancellationToken);

            try
            {
                var cacheEntry = await memoryCache.GetOrCreate(CacheConstants.CacheKeyContributors, async entry =>
                {
                    var result = new List<GitHubContributor>();

                    var (isSuccessful, gitHubRepositories) = await GetAtcRepositories(cancellationToken);
                    if (isSuccessful)
                    {
                        foreach (var gitHubRepository in gitHubRepositories)
                        {
                            var (isSuccessfulContributors, gitHubContributors) = await GetAtcContributorsByRepository(gitHubRepository.Name, cancellationToken);
                            if (!isSuccessfulContributors)
                            {
                                continue;
                            }

                            foreach (var gitHubContributor in gitHubContributors)
                            {
                                if (result.FirstOrDefault(x => x.Id.Equals(gitHubContributor.Id)) is null &&
                                    !gitHubContributor.Name.Equals("ATCBot", StringComparison.Ordinal))
                                {
                                    result.Add(gitHubContributor);
                                }
                            }
                        }
                    }

                    entry.SetSlidingExpiration(CacheConstants.SlidingExpiration);
                    entry.AbsoluteExpirationRelativeToNow = CacheConstants.AbsoluteExpirationRelativeToNow;
                    return result;
                });

                return (isSuccessful: true, cacheEntry);
            }
            catch
            {
                return (isSuccessful: false, new List<GitHubContributor>());
            }
            finally
            {
                LockObject.Release();
            }
        }

        public async Task<(bool isSuccessful, List<GitHubContributor>)> GetAtcContributorsByRepository(string repositoryName, CancellationToken cancellationToken)
        {
            try
            {
                var httpClient = httpClientFactory.CreateClient(HttpClientConstants.GitHubApiClient);
                var result = await httpClient.GetFromJsonAsync<List<GitHubContributor>>(
                    $"/repos/atc-net/{repositoryName}/contributors",
                    jsonSerializerOptions,
                    cancellationToken);

                return result is null
                    ? (isSuccessful: false, new List<GitHubContributor>())
                    : (isSuccessful: true, result);
            }
            catch
            {
                return (isSuccessful: false, new List<GitHubContributor>());
            }
        }
    }
}