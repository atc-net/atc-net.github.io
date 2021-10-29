using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using AtcWeb.Domain.GitHub.Clients;
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
            GitHubRawClient gitHubRawClient,
            List<GitHubPath> foldersAndFiles,
            string repositoryName,
            string defaultBranchName,
            CancellationToken cancellationToken)
        {
            if (gitHubRawClient is null)
            {
                throw new ArgumentNullException(nameof(gitHubRawClient));
            }

            if (foldersAndFiles is null)
            {
                throw new ArgumentNullException(nameof(foldersAndFiles));
            }

            var data = new RootMetadata();

            data.RawReadme = await GetFileByPath(
                gitHubRawClient,
                foldersAndFiles,
                repositoryName,
                defaultBranchName,
                "README.md",
                cancellationToken);

            return data;
        }

        public static async Task<WorkflowMetadata> LoadWorkflow(
            GitHubRawClient gitHubRawClient,
            List<GitHubPath> foldersAndFiles,
            string repositoryName,
            string defaultBranchName,
            CancellationToken cancellationToken)
        {
            if (gitHubRawClient is null)
            {
                throw new ArgumentNullException(nameof(gitHubRawClient));
            }

            if (foldersAndFiles is null)
            {
                throw new ArgumentNullException(nameof(foldersAndFiles));
            }

            var data = new WorkflowMetadata();

            data.RawPreIntegration = await GetFileByPath(
                gitHubRawClient,
                foldersAndFiles,
                repositoryName,
                defaultBranchName,
                ".github/workflows/pre-integration.yml",
                cancellationToken);

            data.RawPostIntegration = await GetFileByPath(
                gitHubRawClient,
                foldersAndFiles,
                repositoryName,
                defaultBranchName,
                ".github/workflows/post-integration.yml",
                cancellationToken);

            data.RawRelease = await GetFileByPath(
                gitHubRawClient,
                foldersAndFiles,
                repositoryName,
                defaultBranchName,
                ".github/workflows/release.yml",
                cancellationToken);

            return data;
        }

        public static async Task<CodingRulesMetadata> LoadCodingRules(
            GitHubRawClient gitHubRawClient,
            List<GitHubPath> foldersAndFiles,
            string repositoryName,
            string defaultBranchName,
            CancellationToken cancellationToken)
        {
            if (gitHubRawClient is null)
            {
                throw new ArgumentNullException(nameof(gitHubRawClient));
            }

            if (foldersAndFiles is null)
            {
                throw new ArgumentNullException(nameof(foldersAndFiles));
            }

            var data = new CodingRulesMetadata();

            data.RawEditorConfigRoot = await GetFileByPath(
                gitHubRawClient,
                foldersAndFiles,
                repositoryName,
                defaultBranchName,
                ".editorconfig",
                cancellationToken);

            data.RawEditorConfigSrc = await GetFileByPath(
                gitHubRawClient,
                foldersAndFiles,
                repositoryName,
                defaultBranchName,
                "src/.editorconfig",
                cancellationToken);

            data.RawEditorConfigTest = await GetFileByPath(
                gitHubRawClient,
                foldersAndFiles,
                repositoryName,
                defaultBranchName,
                "test/.editorconfig",
                cancellationToken);

            return data;
        }

        public static async Task<DotnetMetadata> LoadDotnet(
            GitHubRawClient gitHubRawClient,
            List<GitHubPath> foldersAndFiles,
            string repositoryName,
            string defaultBranchName,
            CancellationToken cancellationToken)
        {
            if (gitHubRawClient is null)
            {
                throw new ArgumentNullException(nameof(gitHubRawClient));
            }

            if (foldersAndFiles is null)
            {
                throw new ArgumentNullException(nameof(foldersAndFiles));
            }

            var data = new DotnetMetadata();

            var fileSolutionFile = foldersAndFiles.Find(x => x.IsFile && "sln".Equals(x.GetFileExtension(), StringComparison.OrdinalIgnoreCase));
            if (fileSolutionFile is not null)
            {
                var (isSuccessfulSolution, rawSolution) = await gitHubRawClient.GetRawAtcCodeFile(
                    repositoryName,
                    defaultBranchName,
                    fileSolutionFile.Path,
                    cancellationToken);
                if (isSuccessfulSolution)
                {
                    data.RawSolution = rawSolution;
                }
            }

            data.RawDirectoryBuildPropsRoot = await GetFileByPath(
                gitHubRawClient,
                foldersAndFiles,
                repositoryName,
                defaultBranchName,
                "Directory.Build.props",
                cancellationToken);

            data.RawDirectoryBuildPropsSrc = await GetFileByPath(
                gitHubRawClient,
                foldersAndFiles,
                repositoryName,
                defaultBranchName,
                "src/Directory.Build.props",
                cancellationToken);

            data.RawDirectoryBuildPropsTest = await GetFileByPath(
                gitHubRawClient,
                foldersAndFiles,
                repositoryName,
                defaultBranchName,
                "test/Directory.Build.props",
                cancellationToken);

            return data;
        }

        private static async Task<string> GetFileByPath(
            GitHubRawClient gitHubRawClient,
            List<GitHubPath> foldersAndFiles,
            string repositoryName,
            string defaultBranchName,
            string path,
            CancellationToken cancellationToken)
        {
            var gitHubFile = foldersAndFiles.Find(x => x.IsFile && path.Equals(x.Path, StringComparison.OrdinalIgnoreCase));
            if (gitHubFile is not null)
            {
                var (isSuccessful, rawFileContent) = await gitHubRawClient.GetRawAtcCodeFile(repositoryName, defaultBranchName, gitHubFile.Path, cancellationToken);
                if (isSuccessful)
                {
                    return rawFileContent;
                }
            }

            return string.Empty;
        }
    }
}