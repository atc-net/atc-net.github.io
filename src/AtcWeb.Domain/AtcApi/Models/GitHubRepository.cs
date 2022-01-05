using System;

namespace AtcWeb.Domain.AtcApi.Models
{
    public class GitHubRepository
    {
        public string Url { get; set; }

        public long Id { get; set; }

        public string Name { get; set; }

        public string Language { get; set; }

        public bool Private { get; set; }

        public string DefaultBranch { get; set; }

        public string Description { get; set; }

        public string Homepage { get; set; }

        public GitHubLicenseMetadata? License { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }

        public DateTimeOffset? PushedAt { get; set; }

        public override string ToString() => $"{nameof(Name)}: {Name}, {nameof(Language)}: {Language}";
    }
}