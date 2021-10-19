using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using AtcWeb.Domain.GitHub.Models;
using HtmlAgilityPack;

// ReSharper disable InvertIf
namespace AtcWeb.Domain.GitHub
{
    [SuppressMessage("Major Code Smell", "S4457:Parameter validation in \"async\"/\"await\" methods should be wrapped", Justification = "TODO: Fix me")]
    public static class GitHubRepositoryMetadataHelper
    {
        public static async Task<RootMetadata> LoadRoot(
            GitHubHtmlClient gitHubHtmlClient,
            GitHubRawClient gitHubRawClient,
            string repositoryName,
            string defaultBranchName,
            CancellationToken cancellationToken)
        {
            if (gitHubHtmlClient is null)
            {
                throw new ArgumentNullException(nameof(gitHubHtmlClient));
            }

            if (gitHubRawClient is null)
            {
                throw new ArgumentNullException(nameof(gitHubRawClient));
            }

            var data = new RootMetadata();

            (bool isSuccessfulHtmlLandingPage, HtmlDocument htmlLandingPage) = await gitHubHtmlClient.GetHtmlAtcCode(repositoryName, cancellationToken);
            if (isSuccessfulHtmlLandingPage)
            {
                Console.WriteLine($"#LP-OK - {repositoryName}"); // TODO: Remove-debug
                data.Test = htmlLandingPage
                    .DocumentNode
                    .SelectSingleNode("//body")
                    .OuterHtml;
            }

            (bool isSuccessfulReadme, string rawReadme) = await gitHubRawClient.GetRawAtcCodeFile(repositoryName, defaultBranchName, "README.md", cancellationToken);
            if (isSuccessfulReadme)
            {
                data.RawReadme = rawReadme;
            }

            return data;
        }

        public static async Task<WorkflowMetadata> LoadWorkflow(
            GitHubRawClient gitHubRawClient,
            string repositoryName,
            string defaultBranchName,
            CancellationToken cancellationToken)
        {
            if (gitHubRawClient is null)
            {
                throw new ArgumentNullException(nameof(gitHubRawClient));
            }

            var data = new WorkflowMetadata();

            (bool isSuccessfulPreIntegration, string rawPreIntegration) = await gitHubRawClient.GetRawAtcCodeFile(repositoryName, defaultBranchName, ".github/workflows/pre-integration.yml", cancellationToken);
            if (isSuccessfulPreIntegration)
            {
                data.RawPreIntegration = rawPreIntegration;

                (bool isSuccessfulPostIntegration, string rawPostIntegration) = await gitHubRawClient.GetRawAtcCodeFile(repositoryName, defaultBranchName, ".github/workflows/post-integration.yml", cancellationToken);
                if (isSuccessfulPostIntegration)
                {
                    data.RawPostIntegration = rawPostIntegration;
                }

                (bool isSuccessfulRelease, string rawRelease) = await gitHubRawClient.GetRawAtcCodeFile(repositoryName, defaultBranchName, ".github/workflows/release.yml", cancellationToken);
                if (isSuccessfulRelease)
                {
                    data.RawRelease = rawRelease;
                }
            }

            return data;
        }

        public static async Task<CodingRulesMetadata> LoadCodingRules(
            GitHubRawClient gitHubRawClient,
            string repositoryName,
            string defaultBranchName,
            CancellationToken cancellationToken)
        {
            if (gitHubRawClient is null)
            {
                throw new ArgumentNullException(nameof(gitHubRawClient));
            }

            var data = new CodingRulesMetadata();

            (bool isSuccessfulEditorConfigRoot, string rawEditorConfigRoot) = await gitHubRawClient.GetRawAtcCodeFile(repositoryName, defaultBranchName, ".editorconfig", cancellationToken);
            if (isSuccessfulEditorConfigRoot)
            {
                data.RawEditorConfigRoot = rawEditorConfigRoot;

                (bool isSuccessfulEditorConfigSrc, string rawEditorConfigSrc) = await gitHubRawClient.GetRawAtcCodeFile(repositoryName, defaultBranchName, "src/.editorconfig", cancellationToken);
                if (isSuccessfulEditorConfigSrc)
                {
                    data.RawEditorConfigSrc = rawEditorConfigSrc;

                    (bool isSuccessfulEditorConfigTest, string rawEditorConfigTest) = await gitHubRawClient.GetRawAtcCodeFile(repositoryName, defaultBranchName, "test/.editorconfig", cancellationToken);
                    if (isSuccessfulEditorConfigTest)
                    {
                        data.RawEditorConfigTest = rawEditorConfigTest;
                    }
                }
            }

            return data;
        }

        public static async Task<DotnetMetadata> LoadDotnet(
            GitHubRawClient gitHubRawClient,
            string repositoryName,
            string defaultBranchName,
            CancellationToken cancellationToken)
        {
            if (gitHubRawClient is null)
            {
                throw new ArgumentNullException(nameof(gitHubRawClient));
            }

            var dotName = repositoryName
                .PascalCase()
                .Replace("-", ".", StringComparison.Ordinal)
                .Replace("Autoformatter", "AutoFormatter", StringComparison.Ordinal);

            var data = new DotnetMetadata();

            (bool isSuccessfulSolution, string rawSolution) = await gitHubRawClient.GetRawAtcCodeFile(repositoryName, defaultBranchName, dotName + ".sln", cancellationToken);
            if (isSuccessfulSolution)
            {
                data.RawSolution = rawSolution;

                (bool isSuccessfulDirectoryBuildPropsRoot, string rawDirectoryBuildPropsRoot) = await gitHubRawClient.GetRawAtcCodeFile(repositoryName, defaultBranchName, "Directory.Build.props", cancellationToken);
                if (isSuccessfulDirectoryBuildPropsRoot)
                {
                    data.RawDirectoryBuildPropsRoot = rawDirectoryBuildPropsRoot;

                    (bool isSuccessfulDirectoryBuildPropsSrc, string rawDirectoryBuildPropsSrc) = await gitHubRawClient.GetRawAtcCodeFile(repositoryName, defaultBranchName, "src/Directory.Build.props", cancellationToken);
                    if (isSuccessfulDirectoryBuildPropsSrc)
                    {
                        data.RawDirectoryBuildPropsSrc = rawDirectoryBuildPropsSrc;

                        (bool isSuccessfulDirectoryBuildPropsTest, string rawDirectoryBuildPropsTest) = await gitHubRawClient.GetRawAtcCodeFile(repositoryName, defaultBranchName, "test/Directory.Build.props", cancellationToken);
                        if (isSuccessfulDirectoryBuildPropsTest)
                        {
                            data.RawDirectoryBuildPropsTest = rawDirectoryBuildPropsTest;
                        }
                    }
                }
            }

            return data;
        }
    }
}