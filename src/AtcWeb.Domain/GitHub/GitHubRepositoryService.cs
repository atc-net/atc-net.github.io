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

        public async Task<List<Repository>> GetRepositoriesAsync(CancellationToken cancellationToken = default)
        {
            var data = new List<Repository>();
            var (isSuccessfulRepositories, gitHubRepositories) = await gitHubApiClient.GetAtcRepositories(cancellationToken);
            if (!isSuccessfulRepositories)
            {
                return data;
            }

            foreach (var gitHubRepository in gitHubRepositories.OrderBy(x => x.Name).Take(1)) // TODO: TAKE(1) !!!!
            {
                var repository = new Repository(gitHubRepository)
                {
                    FoldersAndFiles = await GetDirectoryMetadata(gitHubRepository.Name, cancellationToken),
                };

                data.Add(repository);
            }

            return data;
        }

        private async Task<DirectoryMetadata> GetDirectoryMetadata(string repositoryName, CancellationToken cancellationToken)
        {
            var (isSuccessful, gitHubPaths) = await gitHubApiClient.GetRootPaths(repositoryName, cancellationToken);
            if (!isSuccessful)
            {
                return new DirectoryMetadata();
            }

            var directoryMetadata = new DirectoryMetadata();
            foreach (var gitHubPath in gitHubPaths)
            {
                if (gitHubPath.IsDirectory)
                {
                    var directoryItem = new DirectoryItem
                    {
                        Name = gitHubPath.Name,
                        Directories = await GetDirectories(gitHubPath.Url, cancellationToken),
                    };

                    directoryMetadata.Directories.Add(directoryItem);
                }
                else if (gitHubPath.IsFile)
                {
                    var fileItem = new FileItem
                    {
                        Name = gitHubPath.Name,
                    };

                    directoryMetadata.Files.Add(fileItem);
                }
            }

            return directoryMetadata;
        }

        private async Task<List<DirectoryItem>> GetDirectories(string url, CancellationToken cancellationToken)
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
                    directoryItem.Directories = await GetDirectories(gitHubPath.Url, cancellationToken);
                    directoryItems.Add(directoryItem);
                }
                else if (gitHubPath.IsFile)
                {
                    var fileItem = new FileItem
                    {
                        Name = gitHubPath.Name,
                    };

                    directoryItem.Files.Add(fileItem);
                }
            }

            return directoryItems;
        }
    }
}