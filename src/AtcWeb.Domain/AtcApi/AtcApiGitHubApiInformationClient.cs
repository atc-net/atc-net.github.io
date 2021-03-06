namespace AtcWeb.Domain.AtcApi;

public class AtcApiGitHubApiInformationClient
{
    private const string BaseAddress = "https://atc-api.azurewebsites.net/github/api-information";

    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "OK.")]
    public async Task<(bool isSuccessful, GitHubApiRateLimits)> GetApiRateLimits(CancellationToken cancellationToken = default)
    {
        const string url = $"{BaseAddress}/rate-limits";

        try
        {
            using var httpClient = new HttpClient();
            var responseMessage = await httpClient.GetAsync(url, cancellationToken);
            if (!responseMessage.IsSuccessStatusCode)
            {
                return (isSuccessful: false, new GitHubApiRateLimits());
            }

            var content = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<GitHubApiRateLimits>(content, JsonSerializerOptionsFactory.Create());
            return result is null
                ? (isSuccessful: false, new GitHubApiRateLimits())
                : (isSuccessful: true, result);
        }
        catch
        {
            return (isSuccessful: false, new GitHubApiRateLimits());
        }
    }
}