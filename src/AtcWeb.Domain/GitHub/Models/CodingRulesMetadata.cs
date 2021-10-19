namespace AtcWeb.Domain.GitHub.Models
{
    public class CodingRulesMetadata
    {
        public string RawEditorConfigRoot { get; set; } = string.Empty;

        public string RawEditorConfigSrc { get; set; } = string.Empty;

        public string RawEditorConfigTest { get; set; } = string.Empty;

        public bool HasEditorConfigRoot => !string.IsNullOrEmpty(RawEditorConfigRoot);

        public bool HasEditorConfigSrc => !string.IsNullOrEmpty(RawEditorConfigSrc);

        public bool HasEditorConfigTest => !string.IsNullOrEmpty(RawEditorConfigTest);
    }
}