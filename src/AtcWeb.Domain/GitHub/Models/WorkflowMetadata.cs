namespace AtcWeb.Domain.GitHub.Models
{
    public class WorkflowMetadata
    {
        public string RawPreIntegration { get; set; } = string.Empty;
        
        public string RawPostIntegration { get; set; } = string.Empty;

        public string RawRelease { get; set; } = string.Empty;

        public bool HasPreIntegration => !string.IsNullOrEmpty(RawPreIntegration);
        
        public bool HasPostIntegration => !string.IsNullOrEmpty(RawPostIntegration);

        public bool HasRelease => !string.IsNullOrEmpty(RawRelease);
    }
}