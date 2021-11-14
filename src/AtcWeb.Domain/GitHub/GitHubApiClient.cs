using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using AtcWeb.Domain.GitHub.Models;
using GitHubApiStatus;
using Microsoft.Extensions.Caching.Memory;
using Octokit;

// ReSharper disable StringLiteralTypo
namespace AtcWeb.Domain.GitHub
{
    public class GitHubApiClient
    {
        private readonly IGitHubClient gitHubClient;
        private readonly IMemoryCache memoryCache;

        public GitHubApiClient(IGitHubClient gitHubClient, IMemoryCache memoryCache)
        {
            this.gitHubClient = gitHubClient ?? throw new ArgumentNullException(nameof(gitHubClient));
            this.memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        }

        [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "OK.")]
        public async Task<(bool isSuccessful, GitHubApiRateLimits?)> GetAtcApiRateLimits()
        {
            var gitHubApiStatusService = new GitHubApiStatusService(
                new AuthenticationHeaderValue("bearer", HttpClientConstants.AtcAccessToken),
                new System.Net.Http.Headers.ProductHeaderValue(HttpClientConstants.AtcOrganizationName));

            try
            {
                var apiRateLimits = await gitHubApiStatusService.GetApiRateLimits();
                return (isSuccessful: true, apiRateLimits);
            }
            catch
            {
                return (isSuccessful: false, null);
            }
        }

        public async Task<(bool isSuccessful, List<Repository>)> GetAtcRepositories()
        {
            try
            {
                const string cacheKey = CacheConstants.CacheKeyRepositories;
                if (memoryCache.TryGetValue(cacheKey, out List<Repository> data))
                {
                    return (isSuccessful: true, data);
                }

                var repositories = await gitHubClient.Repository.GetAllForOrg(HttpClientConstants.AtcOrganizationName);

                var filteredRepositories = repositories
                    .Where(x =>
                        !x.Name.Equals("atc-dummy", StringComparison.Ordinal) &&
                        !x.Name.Equals("atc-template-dotnet-package", StringComparison.Ordinal))
                    .ToList();

                memoryCache.Set(cacheKey, filteredRepositories, CacheConstants.AbsoluteExpirationRelativeToNow);
                return (isSuccessful: true, filteredRepositories);
            }
            catch
            {
                return (isSuccessful: false, new List<Repository>());
            }
        }

        public async Task<(bool isSuccessful, Repository?)> GetAtcRepositoryByName(string repositoryName)
        {
            var (isSuccessful, gitHubRepositories) = await GetAtcRepositories();
            if (!isSuccessful)
            {
                return (isSuccessful: false, null);
            }

            var gitHubRepository = gitHubRepositories.SingleOrDefault(x => x.Name.Equals(repositoryName, StringComparison.OrdinalIgnoreCase));

            return gitHubRepository is null
                ? (isSuccessful: false, gitHubRepository: null)
                : (isSuccessful: true, gitHubRepository);
        }

        public async Task<(bool isSuccessful, List<RepositoryContributor>)> GetAtcContributors()
        {
            try
            {
                const string cacheKey = CacheConstants.CacheKeyContributors;
                if (memoryCache.TryGetValue(cacheKey, out List<RepositoryContributor> data))
                {
                    return (isSuccessful: true, data);
                }

                var bag = new ConcurrentBag<RepositoryContributor>();

                var (isSuccessful, gitHubRepositories) = await GetAtcRepositories();
                if (isSuccessful)
                {
                    var tasks = gitHubRepositories
                        .Select(async gitHubRepository =>
                        {
                            var (isSuccessfulContributors, contributors) = await GetAtcContributorsByRepositoryByName(gitHubRepository.Name);
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

                    // TODO: ATC-WhenAll
                    await Task.WhenAll(tasks);
                }

                memoryCache.Set(cacheKey, bag.ToList(), CacheConstants.AbsoluteExpirationRelativeToNow);
                return (isSuccessful: true, bag.ToList());
            }
            catch
            {
                return (isSuccessful: false, new List<RepositoryContributor>());
            }
        }

        public async Task<(bool isSuccessful, List<RepositoryContributor>)> GetAtcContributorsByRepositoryByName(string repositoryName)
        {
            try
            {
                var url = $"/repos/atc-net/{repositoryName}/contributors";
                var cacheKey = $"{CacheConstants.CacheKeyContributors}_{url}";
                if (memoryCache.TryGetValue(cacheKey, out List<RepositoryContributor> data))
                {
                    return (isSuccessful: true, data);
                }

                var contributors = await gitHubClient.Repository.GetAllContributors(HttpClientConstants.AtcOrganizationName, repositoryName);
                if (contributors is null)
                {
                    return (isSuccessful: false, new List<RepositoryContributor>());
                }

                memoryCache.Set(cacheKey, contributors, CacheConstants.AbsoluteExpirationRelativeToNow);
                return (isSuccessful: true, contributors.ToList());
            }
            catch
            {
                return (isSuccessful: false, new List<RepositoryContributor>());
            }
        }

        public async Task<(bool isSuccessful, List<GitHubPath>)> GetAtcAllPathsByRepositoryByName(string repositoryName, string defaultBranchName)
        {
            try
            {
                var url = $"/repos/atc-net/{repositoryName}/git/trees/{defaultBranchName}?recursive=true";
                var cacheKey = $"{CacheConstants.CacheKeyRepositories}_{url}";
                if (memoryCache.TryGetValue(cacheKey, out List<GitHubPath> data))
                {
                    return (isSuccessful: true, data);
                }

                var treeResponse = await gitHubClient.Git.Tree.GetRecursive(HttpClientConstants.AtcOrganizationName, repositoryName, defaultBranchName);

                var gitHubPaths = new List<GitHubPath>();
                foreach (var treeItem in treeResponse.Tree)
                {
                    gitHubPaths.Add(new GitHubPath
                    {
                        Path = treeItem.Path,
                        Url = treeItem.Url,
                        Type = treeItem.Type.ToString(),
                        Sha = treeItem.Sha,
                        Size = treeItem.Size,
                    });
                }

                memoryCache.Set(cacheKey, treeResponse.Tree, CacheConstants.AbsoluteExpirationRelativeToNow);
                return (isSuccessful: true, gitHubPaths);
            }
            catch
            {
                return (isSuccessful: false, new List<GitHubPath>());
            }
        }

        public async Task<(bool isSuccessful, string)> GetRawAtcCodeFile(string repositoryName, string filePath)
        {
            try
            {
                var url = $"/atc-net/{repositoryName}/{filePath}";
                var cacheKey = $"{CacheConstants.CacheKeyCodeFile}_{url}";
                if (memoryCache.TryGetValue(cacheKey, out string data))
                {
                    return (isSuccessful: true, data);
                }

                var rawContent = await gitHubClient.Repository.Content.GetRawContent(HttpClientConstants.AtcOrganizationName, repositoryName, filePath);
                var result = Encoding.UTF8.GetString(rawContent);
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