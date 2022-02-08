namespace AtcWeb.Domain.GitHub.Models;

public class PythonMetadata
{
    public string RawSolution { get; set; } = string.Empty;

    public bool HasSolution => !string.IsNullOrEmpty(RawSolution);
}