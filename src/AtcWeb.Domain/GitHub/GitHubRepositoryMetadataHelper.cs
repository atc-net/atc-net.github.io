// ReSharper disable StringLiteralTypo
// ReSharper disable UseObjectOrCollectionInitializer
namespace AtcWeb.Domain.GitHub;

[SuppressMessage("Major Code Smell", "S4457:Parameter validation in \"async\"/\"await\" methods should be wrapped", Justification = "TODO: Fix me")]
public static class GitHubRepositoryMetadataHelper
{
    public static async Task<RootMetadata> LoadRoot(
        AtcApiGitHubRepositoryClient gitHubRepositoryClient,
        List<GitHubPath> foldersAndFiles,
        string repositoryName,
        string defaultBranchName)
    {
        ArgumentNullException.ThrowIfNull(gitHubRepositoryClient);
        ArgumentNullException.ThrowIfNull(foldersAndFiles);

        var data = new RootMetadata
        {
            RawReadme = await GitHubRepositoryMetadataFileHelper.GetReadMeFile(
                gitHubRepositoryClient,
                foldersAndFiles,
                repositoryName,
                defaultBranchName),
        };

        return data;
    }

    public static async Task<WorkflowMetadata> LoadWorkflow(
        AtcApiGitHubRepositoryClient gitHubRepositoryClient,
        List<GitHubPath> foldersAndFiles,
        string repositoryName)
    {
        ArgumentNullException.ThrowIfNull(gitHubRepositoryClient);
        ArgumentNullException.ThrowIfNull(foldersAndFiles);

        var taskPre = GitHubRepositoryMetadataFileHelper.GetFileByPath(
            gitHubRepositoryClient,
            foldersAndFiles,
            repositoryName,
            ".github/workflows/pre-integration.yml");

        var taskPost = GitHubRepositoryMetadataFileHelper.GetFileByPath(
            gitHubRepositoryClient,
            foldersAndFiles,
            repositoryName,
            ".github/workflows/post-integration.yml");

        var taskRelease = GitHubRepositoryMetadataFileHelper.GetFileByPath(
            gitHubRepositoryClient,
            foldersAndFiles,
            repositoryName,
            ".github/workflows/release.yml");

        await Task.WhenAll(taskPre, taskPost, taskRelease);

        return new WorkflowMetadata
        {
            RawPreIntegration = await taskPre,
            RawPostIntegration = await taskPost,
            RawRelease = await taskRelease,
        };
    }

    public static async Task<CodingRulesMetadata> LoadCodingRules(
        AtcApiGitHubRepositoryClient gitHubRepositoryClient,
        List<GitHubPath> foldersAndFiles,
        string repositoryName)
    {
        ArgumentNullException.ThrowIfNull(gitHubRepositoryClient);
        ArgumentNullException.ThrowIfNull(foldersAndFiles);

        var taskRoot = GitHubRepositoryMetadataFileHelper.GetFileByPath(
            gitHubRepositoryClient, foldersAndFiles, repositoryName, ".editorconfig");

        var taskSrc = GitHubRepositoryMetadataFileHelper.GetFileByPath(
            gitHubRepositoryClient, foldersAndFiles, repositoryName, "src/.editorconfig");

        var taskTest = GitHubRepositoryMetadataFileHelper.GetFileByPath(
            gitHubRepositoryClient, foldersAndFiles, repositoryName, "test/.editorconfig");

        await Task.WhenAll(taskRoot, taskSrc, taskTest);

        return new CodingRulesMetadata
        {
            RawEditorConfigRoot = await taskRoot,
            RawEditorConfigSrc = await taskSrc,
            RawEditorConfigTest = await taskTest,
        };
    }

    public static async Task<List<GitHubIssue>> LoadOpenIssues(
        AtcApiGitHubRepositoryClient gitHubRepositoryClient,
        string repositoryName)
    {
        ArgumentNullException.ThrowIfNull(gitHubRepositoryClient);

        var (isSuccessful, issues) = await gitHubRepositoryClient.GetIssuesOpenByRepositoryByName(repositoryName);
        return isSuccessful
            ? issues
            : new List<GitHubIssue>();
    }

    public static async Task<DotnetMetadata> LoadDotnet(
        AtcApiGitHubRepositoryClient gitHubRepositoryClient,
        List<GitHubPath> foldersAndFiles,
        string repositoryName,
        string defaultBranchName)
    {
        ArgumentNullException.ThrowIfNull(gitHubRepositoryClient);
        ArgumentNullException.ThrowIfNull(foldersAndFiles);

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
                data.SolutionMetadata = VisualStudioSolutionFileHelper.GetSolutionFileMetadata(data.RawSolution);
            }
        }

        var taskBuildPropsRoot = GitHubRepositoryMetadataFileHelper.GetFileByPath(
            gitHubRepositoryClient, foldersAndFiles, repositoryName, "Directory.Build.props");

        var taskBuildPropsSrc = GitHubRepositoryMetadataFileHelper.GetFileByPath(
            gitHubRepositoryClient, foldersAndFiles, repositoryName, "src/Directory.Build.props");

        var taskBuildPropsTest = GitHubRepositoryMetadataFileHelper.GetFileByPath(
            gitHubRepositoryClient, foldersAndFiles, repositoryName, "test/Directory.Build.props");

        await Task.WhenAll(taskBuildPropsRoot, taskBuildPropsSrc, taskBuildPropsTest);

        data.RawDirectoryBuildPropsRoot = await taskBuildPropsRoot;
        data.RawDirectoryBuildPropsSrc = await taskBuildPropsSrc;
        data.RawDirectoryBuildPropsTest = await taskBuildPropsTest;

        data.Projects = await GitHubRepositoryMetadataDotnetHelper.GetProjects(
            gitHubRepositoryClient,
            foldersAndFiles,
            repositoryName,
            defaultBranchName,
            data.RawDirectoryBuildPropsRoot,
            data.RawDirectoryBuildPropsSrc,
            data.RawDirectoryBuildPropsTest);

        return data;
    }

    public static Task<PythonMetadata> LoadPython(
        AtcApiGitHubRepositoryClient gitHubRepositoryClient,
        List<GitHubPath> foldersAndFiles,
        string repositoryName)
    {
        ArgumentNullException.ThrowIfNull(gitHubRepositoryClient);
        ArgumentNullException.ThrowIfNull(foldersAndFiles);

        // TODO: Implement Python metadata loading
        return Task.FromResult(new PythonMetadata());
    }
}