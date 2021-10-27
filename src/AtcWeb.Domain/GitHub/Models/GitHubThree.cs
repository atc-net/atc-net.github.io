using System.Collections.Generic;
using System.Text.Json.Serialization;
using AtcWeb.Domain.GitHub.Models;

namespace AtcWeb.Domain.GitHub.Models
{
    public class GitHubThree
    {
        [JsonPropertyName("tree")]
        public List<GitHubPath> GitHubPaths { get; set; }
    }
}