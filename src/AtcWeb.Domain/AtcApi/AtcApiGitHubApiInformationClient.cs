namespace AtcWeb.Domain.AtcApi;

public class AtcApiGitHubApiInformationClient
{
    private const string BaseAddress = "https://atc-api.azurewebsites.net/github/api-information";

    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "OK.")]
    [SuppressMessage("Minor Code Smell", "S2325:Methods and properties that don't access instance data should be static", Justification = "OK.")]
    public async Task<(bool IsSuccessful, GitHubApiRateLimits GitHubApiRateLimits)> GetApiRateLimits(
        CancellationToken cancellationToken = default)
    {
        const string url = $"{BaseAddress}/rate-limits";

        try
        {
            using var httpClient = new HttpClient();
            var responseMessage = await httpClient.GetAsync(url, cancellationToken);
            if (!responseMessage.IsSuccessStatusCode)
            {
                return (IsSuccessful: false, new GitHubApiRateLimits());
            }

            var content = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<GitHubApiRateLimits>(content, JsonSerializerOptionsFactory.Create());
            return result is null
                ? (IsSuccessful: false, new GitHubApiRateLimits())
                : (IsSuccessful: true, result);
        }
        catch
        {
            return (IsSuccessful: false, new GitHubApiRateLimits());
        }
    }
}