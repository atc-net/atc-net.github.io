namespace AtcWeb.Domain.GitHub.Models;

public class DotnetMetadata
{
    public string RawSolution { get; set; } = string.Empty;

    public string RawDirectoryBuildPropsRoot { get; set; } = string.Empty;

    public string RawDirectoryBuildPropsSrc { get; set; } = string.Empty;

    public string RawDirectoryBuildPropsTest { get; set; } = string.Empty;

    public bool HasSolution => !string.IsNullOrEmpty(RawSolution);

    public bool HasDirectoryBuildPropsRoot => !string.IsNullOrEmpty(RawDirectoryBuildPropsRoot);

    public bool HasDirectoryBuildPropsSrc => !string.IsNullOrEmpty(RawDirectoryBuildPropsSrc);

    public bool HasDirectoryBuildPropsTest => !string.IsNullOrEmpty(RawDirectoryBuildPropsTest);

    public bool IsVisualStudioNameInAcceptedVersion => SolutionMetadata is not null &&
                                                       RepositoryMetadata.IsVisualStudioNameInAcceptedVersion(SolutionMetadata.VisualStudioName);

    public VisualStudioSolutionFileMetadata? SolutionMetadata { get; set; }

    public List<DotnetProject> Projects { get; set; } = new List<DotnetProject>();
}