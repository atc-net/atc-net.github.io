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
        private readonly GitHubRawClient gitHubRawClient;

        public GitHubRepositoryService(GitHubApiClient gitHubApiClient, GitHubRawClient gitHubRawClient)
        {
            this.gitHubApiClient = gitHubApiClient ?? throw new ArgumentNullException(nameof(gitHubApiClient));
            this.gitHubRawClient = gitHubRawClient ?? throw new ArgumentNullException(nameof(gitHubRawClient));
        }

        public async Task<List<GitHubContributor>> GetContributorsAsync(CancellationToken cancellationToken = default)
        {
            var (isSuccessful, gitHubContributors) = await gitHubApiClient.GetAtcContributors(cancellationToken);
            return isSuccessful
                ? gitHubContributors
                : new List<GitHubContributor>();
        }

        public async Task<List<Repository>> GetRepositoriesAsync(bool populateMetaData = false, CancellationToken cancellationToken = default)
        {
            var data = new List<Repository>();
            var (isSuccessfulRepositories, gitHubRepositories) = await gitHubApiClient.GetAtcRepositories(cancellationToken);
            if (!isSuccessfulRepositories)
            {
                return data;
            }

            foreach (var gitHubRepository in gitHubRepositories.OrderBy(x => x.Name))
            {
                var repository = new Repository(gitHubRepository);

                if (populateMetaData)
                {
                    await PopulateMetaData(repository, gitHubRepository, cancellationToken);
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
                await PopulateMetaData(repository, gitHubRepository, cancellationToken);
            }

            return repository;
        }

        private async Task<List<GitHubPath>> GetDirectoryMetadata(string repositoryName, string defaultBranchName, CancellationToken cancellationToken)
        {
            var (isSuccessful, gitHubPaths) = await gitHubApiClient.GetAtcAllPathsByRepository(repositoryName, defaultBranchName, cancellationToken);
            return isSuccessful
                ? gitHubPaths
                : new List<GitHubPath>();
        }

        private async Task PopulateMetaData(Repository repository, GitHubRepository gitHubRepository, CancellationToken cancellationToken)
        {
            repository.FolderAndFilePaths = await GetDirectoryMetadata(
                gitHubRepository.Name,
                repository.DefaultBranchName,
                cancellationToken);

            repository.Root = await GitHubRepositoryMetadataHelper.LoadRoot(
                gitHubRawClient,
                repository.FolderAndFilePaths,
                repository.Name,
                repository.DefaultBranchName,
                cancellationToken);

            repository.Workflow = await GitHubRepositoryMetadataHelper.LoadWorkflow(
                gitHubRawClient,
                repository.FolderAndFilePaths,
                repository.Name,
                repository.DefaultBranchName,
                cancellationToken);

            repository.CodingRules = await GitHubRepositoryMetadataHelper.LoadCodingRules(
                gitHubRawClient,
                repository.FolderAndFilePaths,
                repository.Name,
                repository.DefaultBranchName,
                cancellationToken);

            repository.Dotnet = await GitHubRepositoryMetadataHelper.LoadDotnet(
                gitHubRawClient,
                repository.FolderAndFilePaths,
                repository.Name,
                repository.DefaultBranchName,
                cancellationToken);

            repository.SetBadges();
        }
    }
}
