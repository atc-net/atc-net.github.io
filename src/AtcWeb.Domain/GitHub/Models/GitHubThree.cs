using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AtcWeb.Domain.GitHub.Models
{
    public class GitHubThree
    {
        [JsonPropertyName("tree")]
        public List<GitHubPath> GitHubPaths { get; set; }
    }
}