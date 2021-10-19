namespace AtcWeb.Domain.GitHub.Models
{
    public class DotnetMetadata
    {
        public string RawSolutionFile { get; set; } = string.Empty;

        public bool HasSolutionFile => !string.IsNullOrEmpty(RawSolutionFile);
    }
}