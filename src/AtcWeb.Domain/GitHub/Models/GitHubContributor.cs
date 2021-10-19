using System.Text.Json.Serialization;

namespace AtcWeb.Domain.GitHub.Models
{
    public class GitHubContributor
    {
        public int Id { get; set; }

        [JsonPropertyName("login")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("avatar_url")]
        public string AvatarUrl { get; set; } = string.Empty;

        [JsonPropertyName("html_url")]
        public string Url { get; set; } = string.Empty;

        public override string ToString()
            => $"{nameof(Id)}: {Id}, {nameof(Name)}: {Name}, {nameof(AvatarUrl)}: {AvatarUrl}, {nameof(Url)}: {Url}";
    }
}