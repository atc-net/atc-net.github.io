// ReSharper disable LoopCanBeConvertedToQuery
namespace AtcWeb.Domain.GitHub;

public class GitHubRepositoryService
{
    private readonly AtcApiGitHubApiInformationClient atcApiGitHubApiInformationClient;
    private readonly AtcApiGitHubRepositoryClient atcApiGitHubRepositoryClient;

    public GitHubRepositoryService(
        AtcApiGitHubApiInformationClient atcApiGitHubApiInformationClient,
        AtcApiGitHubRepositoryClient atcApiGitHubRepositoryClient)
    {
        this.atcApiGitHubApiInformationClient = atcApiGitHubApiInformationClient ?? throw new ArgumentNullException(nameof(atcApiGitHubApiInformationClient));
        this.atcApiGitHubRepositoryClient = atcApiGitHubRepositoryClient ?? throw new ArgumentNullException(nameof(atcApiGitHubRepositoryClient));
    }

    public async Task<GitHubApiRateLimits?> GetRestApiRateLimitsAsync()
    {
        var (isSuccessful, gitHubApiRateLimits) = await atcApiGitHubApiInformationClient.GetApiRateLimits();
        return isSuccessful
            ? gitHubApiRateLimits
            : null;
    }

    public async Task<List<GitHubRepositoryContributor>> GetContributorsAsync()
    {
        var (isSuccessful, gitHubContributors) = await atcApiGitHubRepositoryClient.GetContributors();
        return isSuccessful
            ? gitHubContributors
            : new List<GitHubRepositoryContributor>();
    }

    public async Task<List<GitHubRepositoryContributor>> GetResponsibleMembersAsGitHubContributor(string repositoryName)
    {
        var memberNames = RepositoryMetadata.GetResponsibleMembersByName(repositoryName);
        var gitHubContributors = await GetContributorsAsync();
        var data = new List<GitHubRepositoryContributor>();
        foreach (var memberName in memberNames.OrderBy(x => x))
        {
            var gitHubContributor =
                gitHubContributors.Find(x => x.Login.Equals(memberName, StringComparison.OrdinalIgnoreCase));
            if (gitHubContributor is not null)
            {
                data.Add(gitHubContributor);
            }
        }

        return data;
    }

    public async Task<List<AtcRepository>> GetRepositoriesAsync(bool populateMetaDataBase = false, bool populateMetaDataAdvanced = false)
    {
        var bag = new ConcurrentBag<AtcRepository>();
        var (isSuccessfulRepositories, repositories) = await atcApiGitHubRepositoryClient.GetRepositories();
        if (!isSuccessfulRepositories)
        {
            return bag.ToList();
        }

        var tasks = repositories
            .OrderBy(x => x.Name)
            .Select(async repository =>
            {
                var atcRepository = new AtcRepository(repository);

                if (populateMetaDataBase)
                {
                    await PopulateMetaDataBase(atcRepository, repository);

                    if (populateMetaDataAdvanced)
                    {
                        await PopulateMetaDataAdvanced(atcRepository);
                    }
                }

                bag.Add(atcRepository);
            });

        await TaskHelper.WhenAll(tasks);

        var atcRepositories = bag.ToList();
        return atcRepositories;
    }

    public async Task<AtcRepository?> GetRepositoryByNameAsync(string repositoryName, bool populateMetaDataBase = false, bool populateMetaDataAdvanced = false)
    {
        var (isSuccessful, repository) = await atcApiGitHubRepositoryClient.GetRepositoryByName(repositoryName);
        if (!isSuccessful || repository is null)
        {
            return null;
        }

        var atcRepository = new AtcRepository(repository);

        if (populateMetaDataBase)
        {
            await PopulateMetaDataBase(atcRepository, repository);

            if (populateMetaDataAdvanced)
            {
                await PopulateMetaDataAdvanced(atcRepository);
            }
        }

        return atcRepository;
    }

    [SuppressMessage("Minor Code Smell", "S3267:Loops should be simplified with \"LINQ\" expressions", Justification = "OK.")]
    public async Task<List<Tuple<string, string>>> GetDevOpsPlaybook()
    {
        const string repositoryName = "atc-docs";
        var folderAndFilePaths = await GetDirectoryMetadata(repositoryName);

        var data = new List<Tuple<string, string>>();
        foreach (var folderAndFilePath in folderAndFilePaths
                     .Where(x => x.IsFile &&
                                 x.Path.StartsWith("devops-playbook", StringComparison.Ordinal) &&
                                 x.GetFileExtension().Equals("md", StringComparison.Ordinal))
                     .OrderBy(x => x.Path, new NumericAlphaComparer()))
        {
            var rawText = await GitHubRepositoryMetadataFileHelper.GetFileByPathAndEnsureFullLinks(
                atcApiGitHubRepositoryClient,
                folderAndFilePaths,
                repositoryName,
                "main",
                folderAndFilePath.Path);

            var isReadMe = folderAndFilePath.GetFileName().Equals("README.md", StringComparison.Ordinal);
            data.Add(GetDevOpsPlaybookSection(isReadMe, rawText));
        }

        return data;
    }

    private async Task<List<GitHubPath>> GetDirectoryMetadata(string repositoryName)
    {
        var (isSuccessful, gitHubPath) = await atcApiGitHubRepositoryClient.GetAllPathsByRepositoryByName(repositoryName);

        return isSuccessful
            ? gitHubPath
            : new List<GitHubPath>();
    }

    private async Task PopulateMetaDataBase(AtcRepository repository, GitHubRepository gitHubRepository)
    {
        repository.ResponsibleMembers = await GetResponsibleMembersAsGitHubContributor(repository.Name);

        repository.FolderAndFilePaths = await GetDirectoryMetadata(gitHubRepository.Name);

        var taskRoot = GitHubRepositoryMetadataHelper.LoadRoot(
            atcApiGitHubRepositoryClient,
            repository.FolderAndFilePaths,
            repository.Name,
            repository.BaseData.DefaultBranch);

        var taskWorkflow = GitHubRepositoryMetadataHelper.LoadWorkflow(
            atcApiGitHubRepositoryClient,
            repository.FolderAndFilePaths,
            repository.Name);

        var tasks = new List<Task>
        {
            taskRoot, taskWorkflow,
        };

        await TaskHelper.WhenAll(tasks);

        repository.Root = await taskRoot;
        repository.Workflow = await taskWorkflow;

        repository.SetBadges();
    }

    [SuppressMessage("Design", "MA0051:Method is too long", Justification = "OK.")]
    private async Task PopulateMetaDataAdvanced(AtcRepository repository)
    {
        var taskCodingRules = GitHubRepositoryMetadataHelper.LoadCodingRules(
            atcApiGitHubRepositoryClient,
            repository.FolderAndFilePaths,
            repository.Name);

        var taskOpenIssues = GitHubRepositoryMetadataHelper.LoadOpenIssues(
            atcApiGitHubRepositoryClient,
            repository.Name);

        var tasks = new List<Task>
        {
            taskCodingRules, taskOpenIssues,
        };

        Task<DotnetMetadata>? taskDotnet = null;
        Task<PythonMetadata>? taskPython = null;

        if ("C#".Equals(repository.BaseData.Language, StringComparison.Ordinal))
        {
            taskDotnet = GitHubRepositoryMetadataHelper.LoadDotnet(
                atcApiGitHubRepositoryClient,
                repository.FolderAndFilePaths,
                repository.Name,
                repository.BaseData.DefaultBranch);

            tasks.Add(taskDotnet);
        }
        else if ("Python".Equals(repository.BaseData.Language, StringComparison.Ordinal))
        {
            taskPython = GitHubRepositoryMetadataHelper.LoadPython(
                atcApiGitHubRepositoryClient,
                repository.FolderAndFilePaths,
                repository.Name);

            tasks.Add(taskPython);
        }

        await TaskHelper.WhenAll(tasks);

        repository.CodingRules = await taskCodingRules;
        repository.OpenIssues = await taskOpenIssues;

        if ("C#".Equals(repository.BaseData.Language, StringComparison.Ordinal))
        {
            repository.Dotnet = await taskDotnet!;
            var (isSuccessful, latestNugetPackageVersionsUsed) = await atcApiGitHubRepositoryClient.GetLatestNugetPackageVersionsUsed();
            if (isSuccessful)
            {
                foreach (var nugetPackageVersion in repository.Dotnet.Projects.SelectMany(x => x.PackageReferences))
                {
                    var latestNugetPackage = latestNugetPackageVersionsUsed.Find(x => x.PackageId == nugetPackageVersion.PackageId);
                    if (latestNugetPackage is not null &&
                        Version.TryParse(latestNugetPackage.Version, out var latestVersion))
                    {
                        nugetPackageVersion.NewestVersion = latestVersion;
                    }
                }
            }
        }
        else if ("Python".Equals(repository.BaseData.Language, StringComparison.Ordinal))
        {
            repository.Python = await taskPython!;
        }

        repository.SetBadges();
    }

    [SuppressMessage("Design", "MA0051:Method is too long", Justification = "OK.")]
    private static Tuple<string, string> GetDevOpsPlaybookSection(bool isReadMe, string rawText)
    {
        var lines = rawText.ToLines();
        var title = string.Empty;
        var titleIndex = 0;

        for (var i = 0; i < lines.Length; i++)
        {
            if (!lines[i].StartsWith("# ", StringComparison.Ordinal))
            {
                continue;
            }

            title = lines[i].Replace("# ", string.Empty, StringComparison.Ordinal);
            titleIndex = i + 1;
            break;
        }

        var sb = new StringBuilder();
        for (var i = titleIndex; i < lines.Length; i++)
        {
            if (isReadMe && lines[i].StartsWith("# ", StringComparison.Ordinal))
            {
                break;
            }

            var line = lines[i];
            if (line.Trim().Length == 0)
            {
                sb.AppendLine("<br />");
                continue;
            }

            line = line
                .Replace(
                    "(images/",
                    "(https://github.com/atc-net/atc-docs/blob/main/devops-playbook/images/",
                    StringComparison.Ordinal)
                .Replace(
                    "![",
                    "<br />![",
                    StringComparison.Ordinal)
                .Replace(
                    "`TEXT-IS -MISSING`",
                    "`TEXT-IS -MISSING`<br /><br />",
                    StringComparison.Ordinal);

            if (line.StartsWith("**", StringComparison.Ordinal) ||
                line.StartsWith('*'))
            {
                line = "<br /><br />" + line;
            }

            if (line.EndsWith("**", StringComparison.Ordinal) ||
                line.EndsWith(')'))
            {
                line += "<br /><br />";
            }

            sb.AppendLine(line);
        }

        return new Tuple<string, string>(title, sb.ToString());
    }
}