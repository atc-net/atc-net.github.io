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
        private const int MaxLevelDepth = 3;
        private readonly List<string> limitToRootSubFolders = new List<string> { "src", "test" };
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

            foreach (var gitHubRepository in gitHubRepositories.OrderBy(x => x.Name).Take(1)) // TODO: TAKE(1) !!!!
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

        private async Task<DirectoryItem> GetDirectoryMetadata(string repositoryName, int maxLevelDepth, IReadOnlyList<string> limitToRootSubFolders, CancellationToken cancellationToken)
        {
            var (isSuccessful, gitHubPaths) = await gitHubApiClient.GetRootPaths(repositoryName, cancellationToken);
            if (!isSuccessful)
            {
                return new DirectoryItem();
            }

            var directoryItemRoot = new DirectoryItem();
            foreach (var gitHubPath in gitHubPaths)
            {
                if (gitHubPath.IsDirectory)
                {
                    var directoryItem = new DirectoryItem
                    {
                        Name = gitHubPath.Name,
                    };

                    if (maxLevelDepth > 1 && limitToRootSubFolders.Contains(gitHubPath.Name, StringComparer.OrdinalIgnoreCase))
                    {
                        directoryItem.Directories = await GetDirectories(gitHubPath.RootUrl, maxLevelDepth, 2, cancellationToken);
                    }

                    directoryItemRoot.Directories.Add(directoryItem);
                }
                else if (gitHubPath.IsFile)
                {
                    var fileItem = new FileItem { Name = gitHubPath.Name, };

                    directoryItemRoot.Files.Add(fileItem);
                }
            }

            return directoryItemRoot;
        }

        private async Task<List<DirectoryItem>> GetDirectories(string url, int maxLevelDepth, int currentLevelDepth, CancellationToken cancellationToken)
        {
            var (isSuccessful, gitHubPaths) = await gitHubApiClient.GetTreePaths(url, cancellationToken);
            if (!isSuccessful)
            {
                return new List<DirectoryItem>();
            }

            var directoryItems = new List<DirectoryItem>();
            foreach (var gitHubPath in gitHubPaths)
            {
                var directoryItem = new DirectoryItem
                {
                    Name = gitHubPath.Name,
                };

                if (gitHubPath.IsDirectory)
                {
                    if (maxLevelDepth >= currentLevelDepth)
                    {
                        var subDirectoryItems = await GetDirectories(gitHubPath.TreeUrl, maxLevelDepth, currentLevelDepth + 1, cancellationToken);
                        directoryItem.Directories.AddRange(subDirectoryItems);
                    }
                }
                else if (gitHubPath.IsFile)
                {
                    var fileItem = new FileItem
                    {
                        Name = gitHubPath.Name,
                    };

                    directoryItem.Files.Add(fileItem);
                }

                directoryItems.Add(directoryItem);
            }

            return directoryItems;
        }

        private async Task PopulateMetaData(Repository repository, GitHubRepository gitHubRepository, CancellationToken cancellationToken)
        {
            repository.FoldersAndFiles = await GetDirectoryMetadata(
                gitHubRepository.Name,
                MaxLevelDepth,
                limitToRootSubFolders,
                cancellationToken);

            repository.Root = await GitHubRepositoryMetadataHelper.LoadRoot(
                gitHubRawClient,
                repository.FoldersAndFiles,
                repository.Name,
                repository.DefaultBranchName,
                cancellationToken);

            repository.Workflow = await GitHubRepositoryMetadataHelper.LoadWorkflow(
                gitHubRawClient,
                repository.FoldersAndFiles,
                repository.Name,
                repository.DefaultBranchName,
                cancellationToken);

            repository.CodingRules = await GitHubRepositoryMetadataHelper.LoadCodingRules(
                gitHubRawClient,
                repository.FoldersAndFiles,
                repository.Name,
                repository.DefaultBranchName,
                cancellationToken);

            repository.Dotnet = await GitHubRepositoryMetadataHelper.LoadDotnet(
                gitHubRawClient,
                repository.FoldersAndFiles,
                repository.Name,
                repository.DefaultBranchName,
                cancellationToken);

            repository.SetBadges();
        }
    }
}
