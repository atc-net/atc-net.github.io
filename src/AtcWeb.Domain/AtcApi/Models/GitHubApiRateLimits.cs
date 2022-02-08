namespace AtcWeb.Domain.AtcApi.Models;

public class GitHubApiRateLimits
{
    public GitHubApiRateLimitStatus RestApi { get; set; }

    public override string ToString() => $"{nameof(RestApi)}: ({RestApi})";
}