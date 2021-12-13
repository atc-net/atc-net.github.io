using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using AtcWeb.Domain.GitHub.Models;
using AtcWeb.Domain.Nuget;
using Octokit;

// ReSharper disable StringLiteralTypo
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

            data.RawReadme = await GitHubRepositoryMetadataFileHelper.GetReadMeFile(
                gitHubApiClient,
                foldersAndFiles,
                repositoryName,
                defaultBranchName);

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

            data.RawPreIntegration = await GitHubRepositoryMetadataFileHelper.GetFileByPath(
                gitHubApiClient,
                foldersAndFiles,
                repositoryName,
                ".github/workflows/pre-integration.yml");

            data.RawPostIntegration = await GitHubRepositoryMetadataFileHelper.GetFileByPath(
                gitHubApiClient,
                foldersAndFiles,
                repositoryName,
                ".github/workflows/post-integration.yml");

            data.RawRelease = await GitHubRepositoryMetadataFileHelper.GetFileByPath(
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

            data.RawEditorConfigRoot = await GitHubRepositoryMetadataFileHelper.GetFileByPath(
                gitHubApiClient,
                foldersAndFiles,
                repositoryName,
                ".editorconfig");

            data.RawEditorConfigSrc = await GitHubRepositoryMetadataFileHelper.GetFileByPath(
                gitHubApiClient,
                foldersAndFiles,
                repositoryName,
                "src/.editorconfig");

            data.RawEditorConfigTest = await GitHubRepositoryMetadataFileHelper.GetFileByPath(
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

            data.RawDirectoryBuildPropsRoot = await GitHubRepositoryMetadataFileHelper.GetFileByPath(
                gitHubApiClient,
                foldersAndFiles,
                repositoryName,
                "Directory.Build.props");

            data.RawDirectoryBuildPropsSrc = await GitHubRepositoryMetadataFileHelper.GetFileByPath(
                gitHubApiClient,
                foldersAndFiles,
                repositoryName,
                "src/Directory.Build.props");

            data.RawDirectoryBuildPropsTest = await GitHubRepositoryMetadataFileHelper.GetFileByPath(
                gitHubApiClient,
                foldersAndFiles,
                repositoryName,
                "test/Directory.Build.props");

            data.Projects = await GitHubRepositoryMetadataDotnetHelper.GetProjects(
                gitHubApiClient,
                foldersAndFiles,
                repositoryName,
                data.RawDirectoryBuildPropsRoot,
                data.RawDirectoryBuildPropsSrc,
                data.RawDirectoryBuildPropsTest);

            return data;
        }

        public static async Task<List<Issue>> LoadOpenIssues(
            GitHubApiClient gitHubApiClient,
            string repositoryName)
        {
            if (gitHubApiClient is null)
            {
                throw new ArgumentNullException(nameof(gitHubApiClient));
            }

            var (isSuccessful, issues) = await gitHubApiClient.GetIssuesOpenByRepositoryByName(repositoryName);
            return isSuccessful
                ? issues
                : new List<Issue>();
        }
    }
}