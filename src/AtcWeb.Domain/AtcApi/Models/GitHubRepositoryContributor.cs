namespace AtcWeb.Domain.AtcApi.Models
{
    public class GitHubRepositoryContributor
    {
        public string Login { get; set; }

        public int Id { get; set; }

        public string AvatarUrl { get; set; }

        public string Url { get; set; }

        public override string ToString() => $"{nameof(Login)}: {Login}";
    }
}