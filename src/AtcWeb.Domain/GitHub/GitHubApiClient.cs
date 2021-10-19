using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
namespace AtcWeb.Domain.GitHub
{
    public class GitHubApiClient
    {
        private static readonly SemaphoreSlim LockObject = new SemaphoreSlim(1, 1);
        private readonly HttpClient httpClient;
        private readonly IMemoryCache memoryCache;
        private readonly JsonSerializerOptions jsonSerializerOptions;

        [SuppressMessage("Minor Code Smell", "S1075:URIs should not be hardcoded", Justification = "OK.")]
        public GitHubApiClient(HttpClient httpClient, IMemoryCache memoryCache)
        {
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            this.memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            this.httpClient.BaseAddress = new Uri("https://api.github.com"); //// TODO: Introduce typed httpclient..
            this.httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Linux; Android 6.0; Nexus 5 Build/MRA58N) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.106 Mobile Safari/537.36");
            this.jsonSerializerOptions = JsonSerializerOptionsFactory.Create();
        }

        public async Task<(bool isSuccessful, List<GitHubRepository>)> GetAtcRepositories(CancellationToken cancellationToken)
        {
            if (memoryCache.TryGetValue(CacheConstants.CacheKeyRepositories, out List<GitHubRepository> data))
            {
                return (true, data);
            }

            try
            {
                var result = await httpClient.GetFromJsonAsync<List<GitHubRepository>>(
                    "/orgs/atc-net/repos",
                    jsonSerializerOptions,
                    cancellationToken);

                if (result is null)
                {
                    return (false, new List<GitHubRepository>());
                }

                if (result.Count > 0)
                {
                    memoryCache.Set(CacheConstants.CacheKeyRepositories, result);
                }

                return (true, result);
            }
            catch
            {
                return (false, new List<GitHubRepository>());
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

                return (true, cacheEntry);
            }
            catch
            {
                return (false, new List<GitHubContributor>());
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
                var result = await httpClient.GetFromJsonAsync<List<GitHubContributor>>(
                    $"/repos/atc-net/{repositoryName}/contributors",
                    jsonSerializerOptions,
                    cancellationToken);

                return result is null
                    ? (false, new List<GitHubContributor>())
                    : (true, result);
            }
            catch
            {
                return (false, new List<GitHubContributor>());
            }
        }

        public async Task<(bool isSuccessful, string)> GetRawAtcWorkflowFile(string repositoryName, string defaultBranchName, string ymlFile, CancellationToken cancellationToken)
        {
            //// TODO: AddCache

            try
            {
                var result = await httpClient.GetStringAsync(
                    new Uri($"https://raw.githubusercontent.com/atc-net/{repositoryName}/{defaultBranchName}/.github/workflows/{ymlFile}"),
                    cancellationToken);

                return string.IsNullOrEmpty(result)
                    ? (false, string.Empty)
                    : (true, result);
            }
            catch
            {
                return (false, string.Empty);
            }
        }

        public async Task<(bool isSuccessful, string)> GetRawAtcSolutionFile(string repositoryName, string slnFile, string defaultBranchName, CancellationToken cancellationToken)
        {
            //// TODO: AddCache

            try
            {
                var result = await httpClient.GetStringAsync(
                    new Uri($"https://raw.githubusercontent.com/atc-net/{repositoryName}/{defaultBranchName}/{slnFile}"),
                    cancellationToken);

                return string.IsNullOrEmpty(result)
                    ? (false, string.Empty)
                    : (true, result);
            }
            catch
            {
                return (false, string.Empty);
            }
        }
    }
}