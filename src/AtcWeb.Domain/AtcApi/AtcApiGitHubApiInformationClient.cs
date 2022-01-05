using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Atc.Serialization;
using AtcWeb.Domain.AtcApi.Models;

namespace AtcWeb.Domain.AtcApi
{
    public class AtcApiGitHubApiInformationClient
    {
        private const string BaseAddress = "https://atc-api.azurewebsites.net/github/api-information";

        public async Task<(bool isSuccessful, GitHubApiRateLimits)> GetApiRateLimits(CancellationToken cancellationToken = default)
        {
            try
            {
                using var httpClient = new HttpClient();
                var responseMessage = await httpClient.GetAsync(BaseAddress, cancellationToken);
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
}