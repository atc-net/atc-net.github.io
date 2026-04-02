namespace AtcWeb.Domain.AtcApi;

public class AtcApiGitHubApiInformationClient
{
    private const string BaseAddress = $"{AtcApiConstants.BaseAddress}/github/api-information";
    private readonly HttpClient httpClient;

    public AtcApiGitHubApiInformationClient(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);

        this.httpClient = httpClient;
    }

    public async Task<(bool IsSuccessful, GitHubApiRateLimits GitHubApiRateLimits)> GetApiRateLimits(
        CancellationToken cancellationToken = default)
    {
        const string url = $"{BaseAddress}/rate-limits";

        try
        {
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