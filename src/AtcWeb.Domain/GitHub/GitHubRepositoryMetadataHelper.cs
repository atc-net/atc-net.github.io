using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using AtcWeb.Domain.GitHub.Models;

// ReSharper disable InvertIf
// ReSharper disable UseNullPropagation
// ReSharper disable UseObjectOrCollectionInitializer
namespace AtcWeb.Domain.GitHub
{
    [SuppressMessage("Major Code Smell", "S4457:Parameter validation in \"async\"/\"await\" methods should be wrapped", Justification = "TODO: Fix me")]
    public static class GitHubRepositoryMetadataHelper
    {
        public static async Task<RootMetadata> LoadRoot(
            GitHubApiClient gitHubApiClient,
            List<GitHubPath> foldersAndFiles,
            string repositoryName,
            string defaultBranchName)
        {
            if (gitHubApiClient is null)
            {
                throw new ArgumentNullException(nameof(gitHubApiClient));
            }

            if (foldersAndFiles is null)
            {
                throw new ArgumentNullException(nameof(foldersAndFiles));
            }

            var data = new RootMetadata();

            data.RawReadme = await GetFileByPathAndEnsureFullLinks(
                gitHubApiClient,
                foldersAndFiles,
                repositoryName,
                defaultBranchName,
                "README.md");

            return data;
        }

        public static async Task<WorkflowMetadata> LoadWorkflow(
            GitHubApiClient gitHubApiClient,
            List<GitHubPath> foldersAndFiles,
            string repositoryName)
        {
            if (gitHubApiClient is null)
            {
                throw new ArgumentNullException(nameof(gitHubApiClient));
            }

            if (foldersAndFiles is null)
            {
                throw new ArgumentNullException(nameof(foldersAndFiles));
            }

            var data = new WorkflowMetadata();

            data.RawPreIntegration = await GetFileByPath(
                gitHubApiClient,
                foldersAndFiles,
                repositoryName,
                ".github/workflows/pre-integration.yml");

            data.RawPostIntegration = await GetFileByPath(
                gitHubApiClient,
                foldersAndFiles,
                repositoryName,
                ".github/workflows/post-integration.yml");

            data.RawRelease = await GetFileByPath(
                gitHubApiClient,
                foldersAndFiles,
                repositoryName,
                ".github/workflows/release.yml");

            return data;
        }

        public static async Task<CodingRulesMetadata> LoadCodingRules(
            GitHubApiClient gitHubApiClient,
            List<GitHubPath> foldersAndFiles,
            string repositoryName)
        {
            if (gitHubApiClient is null)
            {
                throw new ArgumentNullException(nameof(gitHubApiClient));
            }

            if (foldersAndFiles is null)
            {
                throw new ArgumentNullException(nameof(foldersAndFiles));
            }

            var data = new CodingRulesMetadata();

            data.RawEditorConfigRoot = await GetFileByPath(
                gitHubApiClient,
                foldersAndFiles,
                repositoryName,
                ".editorconfig");

            data.RawEditorConfigSrc = await GetFileByPath(
                gitHubApiClient,
                foldersAndFiles,
                repositoryName,
                "src/.editorconfig");

            data.RawEditorConfigTest = await GetFileByPath(
                gitHubApiClient,
                foldersAndFiles,
                repositoryName,
                "test/.editorconfig");

            return data;
        }

        public static async Task<DotnetMetadata> LoadDotnet(
            GitHubApiClient gitHubApiClient,
            List<GitHubPath> foldersAndFiles,
            string repositoryName)
        {
            if (gitHubApiClient is null)
            {
                throw new ArgumentNullException(nameof(gitHubApiClient));
            }

            if (foldersAndFiles is null)
            {
                throw new ArgumentNullException(nameof(foldersAndFiles));
            }

            var data = new DotnetMetadata();

            var fileSolutionFile = foldersAndFiles.Find(x => x.IsFile && "sln".Equals(x.GetFileExtension(), StringComparison.OrdinalIgnoreCase));
            if (fileSolutionFile is not null)
            {
                var (isSuccessfulSolution, rawSolution) = await gitHubApiClient.GetRawAtcCodeFile(
                    repositoryName,
                    fileSolutionFile.Path);

                if (isSuccessfulSolution)
                {
                    data.RawSolution = rawSolution;
                }
            }

            data.RawDirectoryBuildPropsRoot = await GetFileByPath(
                gitHubApiClient,
                foldersAndFiles,
                repositoryName,
                "Directory.Build.props");

            data.RawDirectoryBuildPropsSrc = await GetFileByPath(
                gitHubApiClient,
                foldersAndFiles,
                repositoryName,
                "src/Directory.Build.props");

            data.RawDirectoryBuildPropsTest = await GetFileByPath(
                gitHubApiClient,
                foldersAndFiles,
                repositoryName,
                "test/Directory.Build.props");

            return data;
        }

        private static async Task<string> GetFileByPath(
            GitHubApiClient gitHubApiClient,
            List<GitHubPath> foldersAndFiles,
            string repositoryName,
            string path)
        {
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

        private static async Task<string> GetFileByPathAndEnsureFullLinks(
            GitHubApiClient gitHubApiClient,
            List<GitHubPath> foldersAndFiles,
            string repositoryName,
            string defaultBranchName,
            string path)
        {
            var gitHubFile = foldersAndFiles.Find(x => x.IsFile && path.Equals(x.Path, StringComparison.OrdinalIgnoreCase));
            if (gitHubFile is not null)
            {
                var (isSuccessful, rawFileContent) = await gitHubApiClient.GetRawAtcCodeFile(repositoryName, gitHubFile.Path);
                if (isSuccessful)
                {
                    rawFileContent = rawFileContent
                        .Replace("](src/", $"](https://github.com/atc-net/{repositoryName}/tree/{defaultBranchName}/src/", StringComparison.Ordinal)
                        .Replace("](docs/", $"](https://github.com/atc-net/{repositoryName}/tree/{defaultBranchName}/docs/", StringComparison.Ordinal);

                    return rawFileContent;
                }
            }

            return string.Empty;
        }
    }
}