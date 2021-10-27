using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using AtcWeb.Domain.GitHub.Clients;
using AtcWeb.Domain.GitHub.Models;

// ReSharper disable InvertIf
// ReSharper disable UseNullPropagation
namespace AtcWeb.Domain.GitHub
{
    [SuppressMessage("Major Code Smell", "S4457:Parameter validation in \"async\"/\"await\" methods should be wrapped", Justification = "TODO: Fix me")]
    public static class GitHubRepositoryMetadataHelper
    {
        public static async Task<RootMetadata> LoadRoot(
            GitHubRawClient gitHubRawClient,
            DirectoryItem foldersAndFiles,
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

            var gitHubReadme = foldersAndFiles.Files.Find(x => "README.md".Equals(x.Name, StringComparison.OrdinalIgnoreCase));
            if (gitHubReadme is not null)
            {
                var (isSuccessfulReadme, rawReadme) = await gitHubRawClient.GetRawAtcCodeFile(repositoryName, defaultBranchName, gitHubReadme.Name, cancellationToken);
                if (isSuccessfulReadme)
                {
                    data.RawReadme = rawReadme;
                }
            }

            return data;
        }

        [SuppressMessage("Design", "MA0051:Method is too long", Justification = "OK.")]
        public static async Task<WorkflowMetadata> LoadWorkflow(
            GitHubRawClient gitHubRawClient,
            DirectoryItem foldersAndFiles,
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

            var folderGitHub = foldersAndFiles.Directories.Find(x => ".github".Equals(x.Name, StringComparison.OrdinalIgnoreCase));
            if (folderGitHub is not null)
            {
                var folderWorkflow = folderGitHub.Directories.Find(x => "workflows".Equals(x.Name, StringComparison.OrdinalIgnoreCase));
                if (folderWorkflow is not null)
                {
                    var filePreIntegration = folderGitHub.Files.Find(x => "pre-integration.yml".Equals(x.Name, StringComparison.OrdinalIgnoreCase));
                    if (filePreIntegration is not null)
                    {
                        var (isSuccessfulPreIntegration, rawPreIntegration) = await gitHubRawClient.GetRawAtcCodeFile(
                            repositoryName,
                            defaultBranchName,
                            ".github/workflows/pre-integration.yml",
                            cancellationToken);
                        if (isSuccessfulPreIntegration)
                        {
                            data.RawPreIntegration = rawPreIntegration;
                        }
                    }

                    var filePostIntegration = folderGitHub.Files.Find(x => "pre-integration.yml".Equals(x.Name, StringComparison.OrdinalIgnoreCase));
                    if (filePostIntegration is not null)
                    {
                        (bool isSuccessfulPostIntegration, string rawPostIntegration) = await gitHubRawClient.GetRawAtcCodeFile(
                            repositoryName,
                            defaultBranchName,
                            ".github/workflows/post-integration.yml",
                            cancellationToken);
                        if (isSuccessfulPostIntegration)
                        {
                            data.RawPostIntegration = rawPostIntegration;
                        }
                    }

                    var fileRelease = folderGitHub.Files.Find(x => "release.yml".Equals(x.Name, StringComparison.OrdinalIgnoreCase));
                    if (fileRelease is not null)
                    {
                        (bool isSuccessfulRelease, string rawRelease) = await gitHubRawClient.GetRawAtcCodeFile(
                            repositoryName,
                            defaultBranchName,
                            ".github/workflows/release.yml", cancellationToken);
                        if (isSuccessfulRelease)
                        {
                            data.RawRelease = rawRelease;
                        }
                    }
                }
            }

            return data;
        }

        public static async Task<CodingRulesMetadata> LoadCodingRules(
            GitHubRawClient gitHubRawClient,
            DirectoryItem foldersAndFiles,
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

            var fileRootEditorConfig = foldersAndFiles.Files.Find(x => ".editorconfig".Equals(x.Name, StringComparison.OrdinalIgnoreCase));
            if (fileRootEditorConfig is not null)
            {
                var (isSuccessfulEditorConfigRoot, rawEditorConfigRoot) = await gitHubRawClient.GetRawAtcCodeFile(
                    repositoryName,
                    defaultBranchName,
                    ".editorconfig",
                    cancellationToken);
                if (isSuccessfulEditorConfigRoot)
                {
                    data.RawEditorConfigRoot = rawEditorConfigRoot;
                }
            }

            var folderSrc = foldersAndFiles.Directories.Find(x => "src".Equals(x.Name, StringComparison.OrdinalIgnoreCase));
            if (folderSrc is not null)
            {
                var (isSuccessfulEditorConfigSrc, rawEditorConfigSrc) = await gitHubRawClient.GetRawAtcCodeFile(
                    repositoryName,
                    defaultBranchName,
                    "src/.editorconfig",
                    cancellationToken);
                if (isSuccessfulEditorConfigSrc)
                {
                    data.RawEditorConfigRoot = rawEditorConfigSrc;
                }
            }

            var folderTest = foldersAndFiles.Directories.Find(x => "test".Equals(x.Name, StringComparison.OrdinalIgnoreCase));
            if (folderTest is not null)
            {
                var (isSuccessfulEditorConfigTest, rawEditorConfigTest) = await gitHubRawClient.GetRawAtcCodeFile(
                    repositoryName,
                    defaultBranchName,
                    "test/.editorconfig",
                    cancellationToken);
                if (isSuccessfulEditorConfigTest)
                {
                    data.RawEditorConfigRoot = rawEditorConfigTest;
                }
            }

            return data;
        }

        [SuppressMessage("Design", "MA0051:Method is too long", Justification = "OK.")]
        public static async Task<DotnetMetadata> LoadDotnet(
            GitHubRawClient gitHubRawClient,
            DirectoryItem foldersAndFiles,
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

            var fileSolutionFile = foldersAndFiles.Files.Find(x => x.Name.EndsWith(".sln", StringComparison.OrdinalIgnoreCase));
            if (fileSolutionFile is not null)
            {
                var (isSuccessfulSolution, rawSolution) = await gitHubRawClient.GetRawAtcCodeFile(
                    repositoryName,
                    defaultBranchName,
                    fileSolutionFile.Name,
                    cancellationToken);
                if (isSuccessfulSolution)
                {
                    data.RawSolution = rawSolution;
                }
            }

            var fileDirectoryBuildPropsRoot = foldersAndFiles.Files.Find(x => "Directory.Build.props".Equals(x.Name, StringComparison.OrdinalIgnoreCase));
            if (fileDirectoryBuildPropsRoot is not null)
            {
                var (isSuccessfulDirectoryBuildPropsRoot, rawDirectoryBuildPropsRoot) = await gitHubRawClient.GetRawAtcCodeFile(
                    repositoryName,
                    defaultBranchName,
                    fileDirectoryBuildPropsRoot.Name,
                    cancellationToken);
                if (isSuccessfulDirectoryBuildPropsRoot)
                {
                    data.RawDirectoryBuildPropsRoot = rawDirectoryBuildPropsRoot;
                }
            }

            var folderSrc = foldersAndFiles.Directories.Find(x => "src".Equals(x.Name, StringComparison.OrdinalIgnoreCase));
            if (folderSrc is not null)
            {
                var fileDirectoryBuildPropsSrc = folderSrc.Files.Find(x => "Directory.Build.props".Equals(x.Name, StringComparison.OrdinalIgnoreCase));
                if (fileDirectoryBuildPropsSrc is not null)
                {
                    var (isSuccessfulDirectoryBuildPropsSrc, rawDirectoryBuildPropsSrc) = await gitHubRawClient.GetRawAtcCodeFile(
                        repositoryName,
                        defaultBranchName,
                        fileDirectoryBuildPropsSrc.Name,
                        cancellationToken);
                    if (isSuccessfulDirectoryBuildPropsSrc)
                    {
                        data.RawDirectoryBuildPropsSrc = rawDirectoryBuildPropsSrc;
                    }
                }
            }

            var folderTest = foldersAndFiles.Directories.Find(x => "test".Equals(x.Name, StringComparison.OrdinalIgnoreCase));
            if (folderTest is not null)
            {
                var fileDirectoryBuildPropsTest = folderTest.Files.Find(x => "Directory.Build.props".Equals(x.Name, StringComparison.OrdinalIgnoreCase));
                if (fileDirectoryBuildPropsTest is not null)
                {
                    var (isSuccessfulDirectoryBuildPropsTest, rawDirectoryBuildPropsTest) = await gitHubRawClient.GetRawAtcCodeFile(
                        repositoryName,
                        defaultBranchName,
                        fileDirectoryBuildPropsTest.Name,
                        cancellationToken);
                    if (isSuccessfulDirectoryBuildPropsTest)
                    {
                        data.RawDirectoryBuildPropsTest = rawDirectoryBuildPropsTest;
                    }
                }
            }

            return data;
        }
    }
}