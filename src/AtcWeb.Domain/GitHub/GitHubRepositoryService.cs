using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AtcWeb.Domain.GitHub.Clients;
using AtcWeb.Domain.GitHub.Models;

namespace AtcWeb.Domain.GitHub
{
    public class GitHubRepositoryService
    {
        private readonly GitHubApiClient gitHubApiClient;
        private readonly GitHubHtmlClient gitHubHtmlClient;
        private readonly GitHubRawClient gitHubRawClient;

        public GitHubRepositoryService(
            GitHubApiClient gitHubApiClient,
            GitHubHtmlClient gitHubHtmlClient,
            GitHubRawClient gitHubRawClient)
        {
            this.gitHubApiClient = gitHubApiClient ?? throw new ArgumentNullException(nameof(gitHubApiClient));
            this.gitHubHtmlClient = gitHubHtmlClient ?? throw new ArgumentNullException(nameof(gitHubHtmlClient));
            this.gitHubRawClient = gitHubRawClient ?? throw new ArgumentNullException(nameof(gitHubRawClient));
        }

        public async Task<List<Repository>> GetRepositoriesAsync(bool populateMetaData = false, CancellationToken cancellationToken = default)
        {
            var data = new List<Repository>();
            var (isSuccessful, gitHubRepositories) = await gitHubApiClient.GetAtcRepositories(cancellationToken);
            if (!isSuccessful)
            {
                return data;
            }

            foreach (var gitHubRepository in gitHubRepositories.OrderBy(x => x.Name))
            {
                var repository = new Repository(gitHubRepository);

                if (populateMetaData)
                {
                    await repository.Load(gitHubApiClient, gitHubHtmlClient, gitHubRawClient, cancellationToken);
                }

                data.Add(repository);
            }

            return data;
        }

        public async Task<Repository?> GetRepositoryByNameAsync(string repositoryName, bool populateMetaData = false, CancellationToken cancellationToken = default)
        {
            var (isSuccessful, gitHubRepository) = await gitHubApiClient.GetAtcRepositoryByName(repositoryName, cancellationToken);
            if (!isSuccessful || gitHubRepository is null)
            {
                return null;
            }

            var repository = new Repository(gitHubRepository);

            if (populateMetaData)
            {
                await repository.Load(gitHubApiClient, gitHubHtmlClient, gitHubRawClient, cancellationToken);
            }

            return repository;
        }

        public async Task<List<GitHubContributor>> GetContributorsAsync(CancellationToken cancellationToken = default)
        {
            var (isSuccessful, gitHubContributors) = await gitHubApiClient.GetAtcContributors(cancellationToken);
            return isSuccessful
                ? gitHubContributors
                : new List<GitHubContributor>();
        }
    }
}