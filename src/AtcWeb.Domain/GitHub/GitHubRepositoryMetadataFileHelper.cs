using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using AtcWeb.Domain.GitHub.Models;

// ReSharper disable InvertIf
namespace AtcWeb.Domain.GitHub
{
    [SuppressMessage("Major Code Smell", "S4457:Parameter validation in \"async\"/\"await\" methods should be wrapped", Justification = "OK.")]
    public static class GitHubRepositoryMetadataFileHelper
    {
        public static async Task<string> GetFileByPath(
            GitHubApiClient gitHubApiClient,
            List<GitHubPath> foldersAndFiles,
            string repositoryName,
            string path)
        {
            if (gitHubApiClient is null)
            {
                throw new ArgumentNullException(nameof(gitHubApiClient));
            }

            if (foldersAndFiles is null)
            {
                throw new ArgumentNullException(nameof(foldersAndFiles));
            }

            var gitHubFile = foldersAndFiles.Find(x => x.IsFile && path.Equals(x.Path, StringComparison.OrdinalIgnoreCase));
            if (gitHubFile is not null)
            {
                var (isSuccessful, rawFileContent) = await gitHubApiClient.GetRawAtcCodeFile(repositoryName, gitHubFile.Path);
                if (isSuccessful)
                {
                    return rawFileContent;
                }
            }

            return string.Empty;
        }

        public static async Task<string> GetFileByPathAndEnsureFullLinks(
            GitHubApiClient gitHubApiClient,
            List<GitHubPath> foldersAndFiles,
            string repositoryName,
            string defaultBranchName,
            string path)
        {
            if (gitHubApiClient is null)
            {
                throw new ArgumentNullException(nameof(gitHubApiClient));
            }

            if (foldersAndFiles is null)
            {
                throw new ArgumentNullException(nameof(foldersAndFiles));
            }

            var gitHubFile = foldersAndFiles.Find(x => x.IsFile && path.Equals(x.Path, StringComparison.OrdinalIgnoreCase));
            if (gitHubFile is not null)
            {
                var (isSuccessful, rawFileContent) = await gitHubApiClient.GetRawAtcCodeFile(repositoryName, gitHubFile.Path);
                if (isSuccessful)
                {
                    rawFileContent = rawFileContent
                        .Replace(
                            "](src/",
                            $"](https://github.com/atc-net/{repositoryName}/tree/{defaultBranchName}/src/",
                            StringComparison.Ordinal)
                        .Replace(
                            "](docs/",
                            $"](https://github.com/atc-net/{repositoryName}/tree/{defaultBranchName}/docs/",
                            StringComparison.Ordinal)
                        .Replace(
                            $"https://github.com/atc-net/{repositoryName}/tree/{defaultBranchName}/",
                            $"https://raw.githubusercontent.com/atc-net/{repositoryName}/{defaultBranchName}/",
                            StringComparison.Ordinal);

                    return rawFileContent;
                }
            }

            return string.Empty;
        }
    }
}