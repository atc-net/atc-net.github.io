using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using AtcWeb.Domain.AtcApi;
using AtcWeb.Domain.AtcApi.Models;
using AtcWeb.Domain.GitHub.Models;

// ReSharper disable StringLiteralTypo
// ReSharper disable UseObjectOrCollectionInitializer
namespace AtcWeb.Domain.GitHub
{
    [SuppressMessage("Major Code Smell", "S4457:Parameter validation in \"async\"/\"await\" methods should be wrapped", Justification = "TODO: Fix me")]
    public static class GitHubRepositoryMetadataHelper
    {
        public static async Task<RootMetadata> LoadRoot(
            AtcApiGitHubRepositoryClient gitHubRepositoryClient,
            List<GitHubPath> foldersAndFiles,
            string repositoryName,
            string defaultBranchName)
        {
            if (gitHubRepositoryClient is null)
            {
                throw new ArgumentNullException(nameof(gitHubRepositoryClient));
            }

            if (foldersAndFiles is null)
            {
                throw new ArgumentNullException(nameof(foldersAndFiles));
            }

            var data = new RootMetadata();

            data.RawReadme = await GitHubRepositoryMetadataFileHelper.GetReadMeFile(
                gitHubRepositoryClient,
                foldersAndFiles,
                repositoryName,
                defaultBranchName);

            return data;
        }

        public static async Task<WorkflowMetadata> LoadWorkflow(
            AtcApiGitHubRepositoryClient gitHubRepositoryClient,
            List<GitHubPath> foldersAndFiles,
            string repositoryName)
        {
            if (gitHubRepositoryClient is null)
            {
                throw new ArgumentNullException(nameof(gitHubRepositoryClient));
            }

            if (foldersAndFiles is null)
            {
                throw new ArgumentNullException(nameof(foldersAndFiles));
            }

            var data = new WorkflowMetadata();

            data.RawPreIntegration = await GitHubRepositoryMetadataFileHelper.GetFileByPath(
                gitHubRepositoryClient,
                foldersAndFiles,
                repositoryName,
                ".github/workflows/pre-integration.yml");

            data.RawPostIntegration = await GitHubRepositoryMetadataFileHelper.GetFileByPath(
                gitHubRepositoryClient,
                foldersAndFiles,
                repositoryName,
                ".github/workflows/post-integration.yml");

            data.RawRelease = await GitHubRepositoryMetadataFileHelper.GetFileByPath(
                gitHubRepositoryClient,
                foldersAndFiles,
                repositoryName,
                ".github/workflows/release.yml");

            return data;
        }

        public static async Task<CodingRulesMetadata> LoadCodingRules(
            AtcApiGitHubRepositoryClient gitHubRepositoryClient,
            List<GitHubPath> foldersAndFiles,
            string repositoryName)
        {
            if (gitHubRepositoryClient is null)
            {
                throw new ArgumentNullException(nameof(gitHubRepositoryClient));
            }

            if (foldersAndFiles is null)
            {
                throw new ArgumentNullException(nameof(foldersAndFiles));
            }

            var data = new CodingRulesMetadata();

            data.RawEditorConfigRoot = await GitHubRepositoryMetadataFileHelper.GetFileByPath(
                gitHubRepositoryClient,
                foldersAndFiles,
                repositoryName,
                ".editorconfig");

            data.RawEditorConfigSrc = await GitHubRepositoryMetadataFileHelper.GetFileByPath(
                gitHubRepositoryClient,
                foldersAndFiles,
                repositoryName,
                "src/.editorconfig");

            data.RawEditorConfigTest = await GitHubRepositoryMetadataFileHelper.GetFileByPath(
                gitHubRepositoryClient,
                foldersAndFiles,
                repositoryName,
                "test/.editorconfig");

            return data;
        }

        public static async Task<List<GitHubIssue>> LoadOpenIssues(
            AtcApiGitHubRepositoryClient gitHubRepositoryClient,
            string repositoryName)
        {
            if (gitHubRepositoryClient is null)
            {
                throw new ArgumentNullException(nameof(gitHubRepositoryClient));
            }

            var (isSuccessful, issues) = await gitHubRepositoryClient.GetIssuesOpenByRepositoryByName(repositoryName);
            return isSuccessful
                ? issues
                : new List<GitHubIssue>();
        }

        public static async Task<DotnetMetadata> LoadDotnet(
            AtcApiGitHubRepositoryClient gitHubRepositoryClient,
            List<GitHubPath> foldersAndFiles,
            string repositoryName)
        {
            if (gitHubRepositoryClient is null)
            {
                throw new ArgumentNullException(nameof(gitHubRepositoryClient));
            }

            if (foldersAndFiles is null)
            {
                throw new ArgumentNullException(nameof(foldersAndFiles));
            }

            var data = new DotnetMetadata();

            var fileSolutionFile = foldersAndFiles.Find(x => x.IsFile &&
                                                             "sln".Equals(x.GetFileExtension(), StringComparison.OrdinalIgnoreCase));
            if (fileSolutionFile is not null)
            {
                var (isSuccessfulSolution, rawSolution) = await gitHubRepositoryClient.GetFileByRepositoryNameAndFilePath(
                    repositoryName,
                    fileSolutionFile.Path);

                if (isSuccessfulSolution)
                {
                    data.RawSolution = rawSolution;
                }
            }

            data.RawDirectoryBuildPropsRoot = await GitHubRepositoryMetadataFileHelper.GetFileByPath(
                gitHubRepositoryClient,
                foldersAndFiles,
                repositoryName,
                "Directory.Build.props");

            data.RawDirectoryBuildPropsSrc = await GitHubRepositoryMetadataFileHelper.GetFileByPath(
                gitHubRepositoryClient,
                foldersAndFiles,
                repositoryName,
                "src/Directory.Build.props");

            data.RawDirectoryBuildPropsTest = await GitHubRepositoryMetadataFileHelper.GetFileByPath(
                gitHubRepositoryClient,
                foldersAndFiles,
                repositoryName,
                "test/Directory.Build.props");

            data.Projects = await GitHubRepositoryMetadataDotnetHelper.GetProjects(
                gitHubRepositoryClient,
                foldersAndFiles,
                repositoryName,
                data.RawDirectoryBuildPropsRoot,
                data.RawDirectoryBuildPropsSrc,
                data.RawDirectoryBuildPropsTest);

            return data;
        }

        public static async Task<PythonMetadata> LoadPython(
            AtcApiGitHubRepositoryClient gitHubRepositoryClient,
            List<GitHubPath> foldersAndFiles,
            string repositoryName)
        {
            if (gitHubRepositoryClient is null)
            {
                throw new ArgumentNullException(nameof(gitHubRepositoryClient));
            }

            if (foldersAndFiles is null)
            {
                throw new ArgumentNullException(nameof(foldersAndFiles));
            }

            var data = new PythonMetadata();

            // TODO:
            await Task.Delay(1);

            return data;
        }
    }
}