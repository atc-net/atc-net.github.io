using System;
using System.Collections.Concurrent;
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
            ////await LockObject.WaitAsync(cancellationToken);

            try
            {
                const string url = "/orgs/atc-net/repos";
                const string cacheKey = CacheConstants.CacheKeyRepositories;
                if (memoryCache.TryGetValue(cacheKey, out List<GitHubRepository> data))
                {
                    return (isSuccessful: true, data);
                }

                var httpClient = httpClientFactory.CreateClient(HttpClientConstants.GitHubApiClient);
                var result = await httpClient.GetFromJsonAsync<List<GitHubRepository>>(url, jsonSerializerOptions, cancellationToken);
                if (result is null)
                {
                    return (isSuccessful: false, new List<GitHubRepository>());
                }

                result = result
                    .Where(x =>
                        !x.Name.Equals("atc-dummy", StringComparison.Ordinal) &&
                        !x.Name.Equals("atc-template-dotnet-package", StringComparison.Ordinal))
                    .ToList();

                memoryCache.Set(cacheKey, result, CacheConstants.AbsoluteExpirationRelativeToNow);
                return (isSuccessful: true, result);
            }
            catch
            {
                return (isSuccessful: false, new List<GitHubRepository>());
            }
            finally
            {
                ////LockObject.Release();
            }
        }

        public async Task<(bool isSuccessful, GitHubRepository?)> GetAtcRepositoryByName(string repositoryName, CancellationToken cancellationToken)
        {
            var (isSuccessful, gitHubRepositories) = await GetAtcRepositories(cancellationToken);
            if (!isSuccessful)
            {
                return (isSuccessful: false, null);
            }

            var gitHubRepository = gitHubRepositories.SingleOrDefault(x => x.Name.Equals(repositoryName, StringComparison.OrdinalIgnoreCase));

            return gitHubRepository is null
                ? (isSuccessful: false, gitHubRepository: null)
                : (isSuccessful: true, gitHubRepository);
        }

        public async Task<(bool isSuccessful, List<GitHubContributor>)> GetAtcContributors(CancellationToken cancellationToken)
        {
            ////await LockObject.WaitAsync(cancellationToken);

            try
            {
                const string cacheKey = CacheConstants.CacheKeyContributors;
                if (memoryCache.TryGetValue(cacheKey, out List<GitHubContributor> data))
                {
                    return (isSuccessful: true, data);
                }

                var bag = new ConcurrentBag<GitHubContributor>();

                var (isSuccessful, gitHubRepositories) = await GetAtcRepositories(cancellationToken);
                if (isSuccessful)
                {
                    var tasks = gitHubRepositories
                        .Select(async gitHubRepository =>
                        {
                            var (isSuccessfulContributors, gitHubContributors) = await GetAtcContributorsByRepositoryByName(gitHubRepository.Name, cancellationToken);
                            if (isSuccessfulContributors)
                            {
                                foreach (var gitHubContributor in gitHubContributors
                                    .Where(gitHubContributor =>
                                        bag.FirstOrDefault(x => x.Id.Equals(gitHubContributor.Id)) is null &&
                                        !gitHubContributor.Name.Equals("ATCBot", StringComparison.Ordinal)))
                                {
                                    bag.Add(gitHubContributor);
                                }
                            }
                        });

                    // TODO: ATC-WhenAll
                    await Task.WhenAll(tasks);
                }

                memoryCache.Set(cacheKey, bag.ToList(), CacheConstants.AbsoluteExpirationRelativeToNow);
                return (isSuccessful: true, bag.ToList());
            }
            catch
            {
                return (isSuccessful: false, new List<GitHubContributor>());
            }
            finally
            {
                ////LockObject.Release();
            }
        }

        public async Task<(bool isSuccessful, List<GitHubContributor>)> GetAtcContributorsByRepositoryByName(string repositoryName, CancellationToken cancellationToken)
        {
            ////await LockObject.WaitAsync(cancellationToken);

            try
            {
                var url = $"/repos/atc-net/{repositoryName}/contributors";
                var cacheKey = $"{CacheConstants.CacheKeyContributors}_{url}";
                if (memoryCache.TryGetValue(cacheKey, out List<GitHubContributor> data))
                {
                    return (isSuccessful: true, data);
                }

                var httpClient = httpClientFactory.CreateClient(HttpClientConstants.GitHubApiClient);
                var result = await httpClient.GetFromJsonAsync<List<GitHubContributor>>(url, jsonSerializerOptions, cancellationToken);
                if (result is null)
                {
                    return (isSuccessful: false, new List<GitHubContributor>());
                }

                memoryCache.Set(cacheKey, result, CacheConstants.AbsoluteExpirationRelativeToNow);
                return (isSuccessful: true, result);
            }
            catch
            {
                return (isSuccessful: false, new List<GitHubContributor>());
            }
            finally
            {
                ////LockObject.Release();
            }
        }

        public async Task<(bool isSuccessful, List<GitHubPath>)> GetAtcAllPathsByRepositoryByName(string repositoryName, string defaultBranchName, CancellationToken cancellationToken)
        {
            ////await LockObject.WaitAsync(cancellationToken);

            try
            {
                var url = $"/repos/atc-net/{repositoryName}/git/trees/{defaultBranchName}?recursive=true";
                var cacheKey = $"{CacheConstants.CacheKeyRepositories}_{url}";
                if (memoryCache.TryGetValue(cacheKey, out List<GitHubPath> data))
                {
                    return (isSuccessful: true, data);
                }

                var httpClient = httpClientFactory.CreateClient(HttpClientConstants.GitHubApiClient);
                var result = await httpClient.GetFromJsonAsync<GitHubThree>(url, jsonSerializerOptions, cancellationToken);
                if (result is null)
                {
                    return (isSuccessful: false, new List<GitHubPath>());
                }

                memoryCache.Set(cacheKey, result.GitHubPaths, CacheConstants.AbsoluteExpirationRelativeToNow);
                return (isSuccessful: true, result.GitHubPaths);
            }
            catch
            {
                return (isSuccessful: false, new List<GitHubPath>());
            }
            finally
            {
                ////LockObject.Release();
            }
        }
    }
}