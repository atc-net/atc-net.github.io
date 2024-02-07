// ReSharper disable InvertIf
namespace AtcWeb.Domain.GitHub;

[SuppressMessage("Major Code Smell", "S4457:Parameter validation in \"async\"/\"await\" methods should be wrapped", Justification = "OK.")]
public static class GitHubRepositoryMetadataFileHelper
{
    public static async Task<string> GetFileByPath(
        AtcApiGitHubRepositoryClient gitHubRepositoryClient,
        List<GitHubPath> foldersAndFiles,
        string repositoryName,
        string path)
    {
        ArgumentNullException.ThrowIfNull(gitHubRepositoryClient);
        ArgumentNullException.ThrowIfNull(foldersAndFiles);

        var gitHubFile = foldersAndFiles.Find(x => x.IsFile && path.Equals(x.Path, StringComparison.OrdinalIgnoreCase));
        if (gitHubFile is not null)
        {
            var (isSuccessful, rawFileContent) = await gitHubRepositoryClient.GetFileByRepositoryNameAndFilePath(repositoryName, gitHubFile.Path);
            if (isSuccessful)
            {
                return rawFileContent;
            }
        }

        return string.Empty;
    }

    public static async Task<string> GetFileByPathAndEnsureFullLinks(
        AtcApiGitHubRepositoryClient gitHubRepositoryClient,
        List<GitHubPath> foldersAndFiles,
        string repositoryName,
        string defaultBranchName,
        string path)
    {
        ArgumentNullException.ThrowIfNull(gitHubRepositoryClient);
        ArgumentNullException.ThrowIfNull(foldersAndFiles);

        var gitHubFile = foldersAndFiles.Find(x => x.IsFile && path.Equals(x.Path, StringComparison.OrdinalIgnoreCase));
        if (gitHubFile is not null)
        {
            var (isSuccessful, rawFileContent) = await gitHubRepositoryClient.GetFileByRepositoryNameAndFilePath(repositoryName, gitHubFile.Path);
            if (isSuccessful)
            {
                rawFileContent = rawFileContent
                    .Replace(
                        "](src/",
                        $"](https://github.com/atc-net/{repositoryName}/tree/{defaultBranchName}/src/",
                        StringComparison.Ordinal)
                    .Replace(
                        "](docs/",
                        $"](https://github.com/atc-net/{repositoryName}/blob/{defaultBranchName}/docs/",
                        StringComparison.Ordinal);

                return rawFileContent;
            }
        }

        return string.Empty;
    }

    public static async Task<string> GetReadMeFile(
        AtcApiGitHubRepositoryClient gitHubRepositoryClient,
        List<GitHubPath> foldersAndFiles,
        string repositoryName,
        string defaultBranchName,
        string path = "README.md")
    {
        var rawText = await GetFileByPathAndEnsureFullLinks(
            gitHubRepositoryClient,
            foldersAndFiles,
            repositoryName,
            defaultBranchName,
            path);

        if (string.IsNullOrEmpty(rawText))
        {
            return rawText;
        }

        var lines = rawText.Split(Environment.NewLine);
        var sb = new StringBuilder();
        var append = false;
        var lastLine = string.Empty;
        foreach (var line in lines)
        {
            // Skip all until first "H1"
            if (line.StartsWith("# ", StringComparison.Ordinal))
            {
                append = true;
            }

            if (append)
            {
                // Table - a Table need to start with a blank line in markdown to be valid.
                if (line.StartsWith('|') &&
                    (lastLine.Trim().Length != 0 && !lastLine.StartsWith('|')))
                {
                    sb.AppendLine();
                }

                sb.AppendLine(line);
            }

            lastLine = line;
        }

        return sb.ToString();
    }
}