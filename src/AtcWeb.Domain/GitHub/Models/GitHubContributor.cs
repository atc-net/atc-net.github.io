using System.Text.Json.Serialization;

namespace AtcWeb.Domain.GitHub.Models
{
    public class GitHubContributor
    {
        public int Id { get; set; }

        [JsonPropertyName("login")]
        public string Name { get; set; }

        [JsonPropertyName("avatar_url")]
        public string AvatarUrl { get; set; }

        [JsonPropertyName("html_url")]
        public string Url { get; set; }

        public override string ToString()
            => $"{nameof(Id)}: {Id}, {nameof(Name)}: {Name}, {nameof(AvatarUrl)}: {AvatarUrl}, {nameof(Url)}: {Url}";
    }
}