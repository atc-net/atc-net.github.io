// ReSharper disable ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
// ReSharper disable LoopCanBeConvertedToQuery
namespace AtcWeb.Domain.GitHub;

public class GitHubRepositoryService
{
    private readonly AtcApiGitHubApiInformationClient atcApiGitHubApiInformationClient;
    private readonly AtcApiGitHubRepositoryClient atcApiGitHubRepositoryClient;
    private readonly AtcApiGitHubWikiClient atcApiGitHubWikiClient;

    public GitHubRepositoryService(
        AtcApiGitHubApiInformationClient atcApiGitHubApiInformationClient,
        AtcApiGitHubRepositoryClient atcApiGitHubRepositoryClient,
        AtcApiGitHubWikiClient atcApiGitHubWikiClient)
    {
        ArgumentNullException.ThrowIfNull(atcApiGitHubApiInformationClient);
        ArgumentNullException.ThrowIfNull(atcApiGitHubRepositoryClient);
        ArgumentNullException.ThrowIfNull(atcApiGitHubWikiClient);

        this.atcApiGitHubApiInformationClient = atcApiGitHubApiInformationClient;
        this.atcApiGitHubRepositoryClient = atcApiGitHubRepositoryClient;
        this.atcApiGitHubWikiClient = atcApiGitHubWikiClient;
    }

    public async Task<GitHubApiRateLimits?> GetRestApiRateLimitsAsync()
    {
        var (isSuccessful, gitHubApiRateLimits) = await atcApiGitHubApiInformationClient.GetApiRateLimits();
        return isSuccessful
            ? gitHubApiRateLimits
            : null;
    }

    public async Task<List<AtcRepository>> GetRepositoriesAsync(
        bool populateMetaDataBase = false,
        bool populateMetaDataAdvanced = false)
    {
        var bag = new ConcurrentBag<AtcRepository>();
        var (isSuccessfulRepositories, repositories) = await atcApiGitHubRepositoryClient.GetRepositories();
        if (!isSuccessfulRepositories)
        {
            return bag.ToList();
        }

        var tasks = repositories
            .OrderBy(x => x.Name, StringComparer.Ordinal)
            .Select(async repository =>
            {
                var atcRepository = new AtcRepository(repository);

                if (populateMetaDataBase)
                {
                    await PopulateMetaDataBaseAsync(atcRepository, repository);

                    if (populateMetaDataAdvanced)
                    {
                        await PopulateMetaDataAdvancedAsync(atcRepository);
                    }
                }

                bag.Add(atcRepository);
            });

        await TaskHelper.WhenAll(tasks);

        var atcRepositories = bag.ToList();
        return atcRepositories;
    }

    public async Task<AtcRepository?> GetRepositoryByNameAsync(
        string repositoryName,
        bool populateMetaDataBase = false,
        bool populateMetaDataAdvanced = false)
    {
        var (isSuccessful, repository) = await atcApiGitHubRepositoryClient.GetRepositoryByName(repositoryName);
        if (!isSuccessful || repository is null)
        {
            return null;
        }

        var atcRepository = new AtcRepository(repository);

        if (populateMetaDataBase)
        {
            await PopulateMetaDataBaseAsync(atcRepository, repository);

            if (populateMetaDataAdvanced)
            {
                await PopulateMetaDataAdvancedAsync(atcRepository);
            }
        }

        return atcRepository;
    }

    [SuppressMessage("Minor Code Smell", "S3267:Loops should be simplified with \"LINQ\" expressions", Justification = "OK.")]
    public async Task<DocumentMetadata> GetDevOpsPlaybook()
    {
        const string repositoryName = "atc-docs";
        var folderAndFilePaths = await GetDirectoryMetadataAsync(repositoryName);

        var documentMetadata = new DocumentMetadata();
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
            documentMetadata.SubSection.Add(GetDevOpsPlaybookSection(isReadMe, rawText));
        }

        return documentMetadata;
    }

    public async Task<List<NewsItem>> GetNews()
    {
        var news = new List<NewsItem>();
        news.AddRange(NewsMetadata.GetNews());

        var repositories = await GetRepositoriesAsync(populateMetaDataBase: true);
        foreach (var atcRepository in repositories)
        {
            news.Add(
                new NewsItem(
                    atcRepository.BaseData.CreatedAt,
                    NewsItemAction.RepositoryNew,
                    atcRepository.Name,
                    string.Empty,
                    string.Empty));
        }

        return news
            .OrderByDescending(x => x.Time)
            .ToList();
    }

    public async Task<List<GitHubPath>> GetDirectoryMetadataAsync(
        string repositoryName)
    {
        var (isSuccessful, gitHubPath) = await atcApiGitHubRepositoryClient.GetAllPathsByRepositoryByName(repositoryName);

        return isSuccessful
            ? gitHubPath
            : new List<GitHubPath>();
    }

    public async Task PopulateMetaDataBaseAsync(
        AtcRepository repository,
        GitHubRepository gitHubRepository)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(gitHubRepository);

        repository.FolderAndFilePaths = await GetDirectoryMetadataAsync(gitHubRepository.Name);

        repository.Root = await GitHubRepositoryMetadataHelper.LoadRoot(
            atcApiGitHubRepositoryClient,
            repository.FolderAndFilePaths,
            repository.Name,
            repository.BaseData.DefaultBranch);
    }

    public async Task PopulateReadmeAsync(AtcRepository repository)
    {
        ArgumentNullException.ThrowIfNull(repository);

        repository.Root = await GitHubRepositoryMetadataHelper.LoadRoot(
            atcApiGitHubRepositoryClient,
            repository.FolderAndFilePaths,
            repository.Name,
            repository.BaseData.DefaultBranch);
    }

    public async Task PopulateWikiAsync(AtcRepository repository)
    {
        ArgumentNullException.ThrowIfNull(repository);

        var (isSuccessful, wiki) = await atcApiGitHubWikiClient.GetWiki(repository.Name);
        if (isSuccessful)
        {
            repository.Wiki = wiki;
        }
    }

    [SuppressMessage("Design", "MA0051:Method is too long", Justification = "OK.")]
    public async Task PopulateMetaDataAdvancedAsync(AtcRepository repository)
    {
        ArgumentNullException.ThrowIfNull(repository);

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
    }

    [SuppressMessage("Design", "MA0051:Method is too long", Justification = "OK.")]
    private static DocumentSectionMetadata GetDevOpsPlaybookSection(
        bool isReadMe,
        string rawText)
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

        var documentSectionMetadata = new DocumentSectionMetadata
        {
            Title = title,
        };

        var sbBody = new StringBuilder();
        var subTitle = string.Empty;
        var sbSubBody = new StringBuilder();
        var workOnSub = false;
        var level = 0;
        for (var i = titleIndex; i < lines.Length; i++)
        {
            if (isReadMe && lines[i].StartsWith("# ", StringComparison.Ordinal))
            {
                break;
            }

            var line = lines[i];

            if (line.StartsWith("# ", StringComparison.Ordinal) ||
                line.StartsWith("## ", StringComparison.Ordinal))
            {
                if (workOnSub && !string.IsNullOrEmpty(subTitle))
                {
                    documentSectionMetadata.SubSection.Add(
                        new DocumentSectionMetadata
                        {
                            Level = level,
                            Title = subTitle,
                            Body = sbSubBody.ToString(),
                        });
                }

                workOnSub = true;
                sbSubBody = new StringBuilder();
                if (line.StartsWith("# ", StringComparison.Ordinal))
                {
                    level = 1;
                    subTitle = line.Replace("# ", string.Empty, StringComparison.Ordinal);
                }
                else if (line.StartsWith("## ", StringComparison.Ordinal))
                {
                    level = 2;
                    subTitle = line.Replace("## ", string.Empty, StringComparison.Ordinal);
                }
            }
            else
            {
                if (line.Trim().Length == 0)
                {
                    if (workOnSub)
                    {
                        sbSubBody.AppendLine("<br />");
                    }
                    else
                    {
                        sbBody.AppendLine("<br />");
                    }
                }
                else
                {
                    line = FormatLine(line);
                    if (workOnSub)
                    {
                        sbSubBody.AppendLine(line);
                    }
                    else
                    {
                        sbBody.AppendLine(line);
                    }
                }
            }
        }

        if (workOnSub && !string.IsNullOrEmpty(subTitle))
        {
            documentSectionMetadata.SubSection.Add(
                new DocumentSectionMetadata
                {
                    Level = level,
                    Title = subTitle,
                    Body = sbSubBody.ToString(),
                });

            sbBody.AppendLine("<br /><br /><br />");
        }

        documentSectionMetadata.Body = sbBody.ToString();

        return documentSectionMetadata;
    }

    private static string FormatLine(string line)
    {
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

        return line;
    }
}