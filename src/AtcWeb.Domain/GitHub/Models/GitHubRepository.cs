using System;
using System.Text.Json.Serialization;

namespace AtcWeb.Domain.GitHub.Models
{
    public class GitHubRepository
    {
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("html_url")]
        public string Url { get; set; }

        [JsonPropertyName("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonPropertyName("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; }

        [JsonPropertyName("pushed_at")]
        public DateTimeOffset PushedAt { get; set; }

        [JsonPropertyName("clone_url")]
        public string CloneUrl { get; set; } = string.Empty;

        [JsonPropertyName("language")]
        public string Language { get; set; } = string.Empty;

        public override string ToString()
            => $"{nameof(Name)}: {Name}, {nameof(Description)}: {Description}, {nameof(Url)}: {Url}, {nameof(CreatedAt)}: {CreatedAt}, {nameof(UpdatedAt)}: {UpdatedAt}, {nameof(PushedAt)}: {PushedAt}, {nameof(CloneUrl)}: {CloneUrl}, {nameof(Language)}: {Language}";
    }
}