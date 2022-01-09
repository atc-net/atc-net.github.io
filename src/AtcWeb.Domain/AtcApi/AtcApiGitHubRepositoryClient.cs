using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Atc.DotNet.Models;
using Atc.Helpers;
using Atc.Serialization;
using AtcWeb.Domain.AtcApi.Models;
using AtcWeb.Domain.GitHub;
using Microsoft.Extensions.Caching.Memory;

namespace AtcWeb.Domain.AtcApi
{
    public class AtcApiGitHubRepositoryClient
    {
        private const string BaseAddress = "https://atc-api.azurewebsites.net/github/repository";
        private readonly IMemoryCache memoryCache;

        public AtcApiGitHubRepositoryClient(IMemoryCache memoryCache)
        {
            this.memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        }

        public async Task<(bool isSuccessful, List<GitHubRepository>)> GetRepositories(CancellationToken cancellationToken = default)
        {
            const string cacheKey = CacheConstants.CacheKeyRepositories;
            if (memoryCache.TryGetValue(cacheKey, out List<GitHubRepository> data))
            {
                return (isSuccessful: true, data);
            }

            try
            {
                const string url = $"{BaseAddress}/";
                using var httpClient = new HttpClient();
                var responseMessage = await httpClient.GetAsync(url, cancellationToken);
                if (!responseMessage.IsSuccessStatusCode)
                {
                    return (isSuccessful: false, new List<GitHubRepository>());
                }

                var content = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
                var result = JsonSerializer.Deserialize<IReadOnlyList<GitHubRepository>>(content, JsonSerializerOptionsFactory.Create());
                if (result is null)
                {
                    return (isSuccessful: false, new List<GitHubRepository>());
                }

                memoryCache.Set(cacheKey, result, CacheConstants.AbsoluteExpirationRelativeToNow);
                return (isSuccessful: true, result.ToList());
            }
            catch
            {
                return (isSuccessful: false, new List<GitHubRepository>());
            }
        }

        public async Task<(bool isSuccessful, GitHubRepository?)> GetRepositoryByName(string repositoryName, CancellationToken cancellationToken = default)
        {
            var (isSuccessful, repositories) = await GetRepositories(cancellationToken);
            if (!isSuccessful)
            {
                return (isSuccessful: false, null);
            }

            var repository = repositories.SingleOrDefault(x => x.Name.Equals(repositoryName, StringComparison.OrdinalIgnoreCase));

            return repository is null
                ? (isSuccessful: false, gitHubRepository: null)
                : (isSuccessful: true, gitHubRepository: repository);
        }

        public async Task<(bool isSuccessful, List<GitHubRepositoryContributor>)> GetContributors(CancellationToken cancellationToken = default)
        {
            const string cacheKey = CacheConstants.CacheKeyContributors;
            if (memoryCache.TryGetValue(cacheKey, out List<GitHubRepositoryContributor> data))
            {
                return (isSuccessful: true, data);
            }

            try
            {
                var bag = new ConcurrentBag<GitHubRepositoryContributor>();

                var (isSuccessful, gitHubRepositories) = await GetRepositories(cancellationToken);
                if (isSuccessful)
                {
                    var tasks = gitHubRepositories
                        .Select(async gitHubRepository =>
                        {
                            var (isSuccessfulContributors, contributors) = await GetContributorsByRepositoryByName(gitHubRepository.Name, cancellationToken);
                            if (isSuccessfulContributors)
                            {
                                foreach (var contributor in contributors
                                             .Where(gitHubContributor =>
                                                 bag.FirstOrDefault(x => x.Id.Equals(gitHubContributor.Id)) is null &&
                                                 !gitHubContributor.Login.Equals("ATCBot", StringComparison.Ordinal)))
                                {
                                    bag.Add(contributor);
                                }
                            }
                        });

                    await TaskHelper.WhenAll(tasks);
                }

                memoryCache.Set(cacheKey, bag.ToList(), CacheConstants.AbsoluteExpirationRelativeToNow);
                return (isSuccessful: true, bag.ToList());
            }
            catch
            {
                return (isSuccessful: false, new List<GitHubRepositoryContributor>());
            }
        }

        public async Task<(bool isSuccessful, List<GitHubRepositoryContributor>)> GetContributorsByRepositoryByName(string repositoryName, CancellationToken cancellationToken = default)
        {
            var url = $"{BaseAddress}/contributors/{repositoryName}";
            var cacheKey = $"{CacheConstants.CacheKeyRepositories}_{url}";
            if (memoryCache.TryGetValue(cacheKey, out List<GitHubRepositoryContributor> data))
            {
                return (isSuccessful: true, data);
            }

            try
            {
                using var httpClient = new HttpClient();
                var responseMessage = await httpClient.GetAsync(url, cancellationToken);
                if (!responseMessage.IsSuccessStatusCode)
                {
                    return (isSuccessful: false, new List<GitHubRepositoryContributor>());
                }

                var content = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
                var result = JsonSerializer.Deserialize<List<GitHubRepositoryContributor>>(content, JsonSerializerOptionsFactory.Create());
                if (result is null)
                {
                    return (isSuccessful: false, new List<GitHubRepositoryContributor>());
                }

                memoryCache.Set(cacheKey, result, CacheConstants.AbsoluteExpirationRelativeToNow);
                return (isSuccessful: true, result.ToList());
            }
            catch
            {
                return (isSuccessful: false, new List<GitHubRepositoryContributor>());
            }
        }

        public async Task<(bool isSuccessful, List<DotnetNugetPackageMetadataBase>)> GetLatestNugetPackageVersionsUsed(CancellationToken cancellationToken = default)
        {
            const string cacheKey = CacheConstants.CacheKeyNugetPackagesUsedByAtcRepositories;
            if (memoryCache.TryGetValue(cacheKey, out List<DotnetNugetPackageMetadataBase> data))
            {
                return (isSuccessful: true, data);
            }

            try
            {
                const string url = $"{BaseAddress}/nuget-packages-used";
                using var httpClient = new HttpClient();
                var responseMessage = await httpClient.GetAsync(url, cancellationToken);
                if (!responseMessage.IsSuccessStatusCode)
                {
                    return (isSuccessful: false, new List<DotnetNugetPackageMetadataBase>());
                }

                var content = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
                var result = JsonSerializer.Deserialize<List<DotnetNugetPackageMetadataBase>>(content, JsonSerializerOptionsFactory.Create());
                if (result is null)
                {
                    return (isSuccessful: false, new List<DotnetNugetPackageMetadataBase>());
                }

                memoryCache.Set(cacheKey, result, CacheConstants.AbsoluteExpirationRelativeToNow);
                return (isSuccessful: true, result);
            }
            catch
            {
                return (isSuccessful: false, new List<DotnetNugetPackageMetadataBase>());
            }
        }

        public async Task<(bool isSuccessful, List<GitHubPath>)> GetAllPathsByRepositoryByName(string repositoryName, CancellationToken cancellationToken = default)
        {
            var url = $"{BaseAddress}/{repositoryName}/paths";
            var cacheKey = $"{CacheConstants.CacheKeyRepositories}_{url}";
            if (memoryCache.TryGetValue(cacheKey, out List<GitHubPath> data))
            {
                return (isSuccessful: true, data);
            }

            try
            {
                using var httpClient = new HttpClient();
                var responseMessage = await httpClient.GetAsync(url, cancellationToken);
                if (!responseMessage.IsSuccessStatusCode)
                {
                    return (isSuccessful: false, new List<GitHubPath>());
                }

                var content = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
                var result = JsonSerializer.Deserialize<List<GitHubPath>>(content, JsonSerializerOptionsFactory.Create());
                if (result is null)
                {
                    return (isSuccessful: false, new List<GitHubPath>());
                }

                memoryCache.Set(cacheKey, result, CacheConstants.AbsoluteExpirationRelativeToNow);
                return (isSuccessful: true, result);
            }
            catch
            {
                return (isSuccessful: false, new List<GitHubPath>());
            }
        }

        public async Task<(bool isSuccessful, string)> GetFileByRepositoryNameAndFilePath(string repositoryName, string filePath, CancellationToken cancellationToken = default)
        {
            var url = $"{BaseAddress}/{repositoryName}/file?filePath={filePath}";
            var cacheKey = $"{CacheConstants.CacheKeyRepositoryFile}_{url}";
            if (memoryCache.TryGetValue(cacheKey, out string data))
            {
                return (isSuccessful: true, data);
            }

            try
            {
                using var httpClient = new HttpClient();
                var responseMessage = await httpClient.GetAsync(url, cancellationToken);
                if (!responseMessage.IsSuccessStatusCode)
                {
                    return (isSuccessful: false, string.Empty);
                }

                var result = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
                if (string.IsNullOrEmpty(result))
                {
                    return (isSuccessful: false, string.Empty);
                }

                memoryCache.Set(cacheKey, result, CacheConstants.AbsoluteExpirationRelativeToNow);
                return (isSuccessful: true, result);
            }
            catch
            {
                return (isSuccessful: false, string.Empty);
            }
        }

        public Task<(bool isSuccessful, List<GitHubIssue>)> GetIssuesAllByRepositoryByName(string repositoryName, CancellationToken cancellationToken = default)
            => GetIssuesByRepositoryByName(repositoryName, "all", cancellationToken);

        public Task<(bool isSuccessful, List<GitHubIssue>)> GetIssuesOpenByRepositoryByName(string repositoryName, CancellationToken cancellationToken = default)
            => GetIssuesByRepositoryByName(repositoryName, "open", cancellationToken);

        public Task<(bool isSuccessful, List<GitHubIssue>)> GetIssuesClosedByRepositoryByName(string repositoryName, CancellationToken cancellationToken = default)
            => GetIssuesByRepositoryByName(repositoryName, "closed", cancellationToken);

        private async Task<(bool isSuccessful, List<GitHubIssue>)> GetIssuesByRepositoryByName(string repositoryName, string state, CancellationToken cancellationToken = default)
        {
            var url = $"{BaseAddress}/{repositoryName}/issues/{state}";
            var cacheKey = $"{CacheConstants.CacheKeyIssues}_{url}";
            if (memoryCache.TryGetValue(cacheKey, out List<GitHubIssue> data))
            {
                return (isSuccessful: true, data);
            }

            try
            {
                using var httpClient = new HttpClient();
                var responseMessage = await httpClient.GetAsync(url, cancellationToken);
                if (!responseMessage.IsSuccessStatusCode)
                {
                    return (isSuccessful: false, new List<GitHubIssue>());
                }

                var content = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
                var result = JsonSerializer.Deserialize<List<GitHubIssue>>(content, JsonSerializerOptionsFactory.Create());
                if (result is null)
                {
                    return (isSuccessful: false, new List<GitHubIssue>());
                }

                memoryCache.Set(cacheKey, result, CacheConstants.AbsoluteExpirationRelativeToNow);
                return (isSuccessful: true, result.ToList());
            }
            catch
            {
                return (isSuccessful: false, new List<GitHubIssue>());
            }
        }
    }
}