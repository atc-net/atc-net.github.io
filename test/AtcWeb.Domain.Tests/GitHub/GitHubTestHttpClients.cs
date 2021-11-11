using System;
using System.Net.Http;

namespace AtcWeb.Domain.Tests.GitHub
{
    public static class GitHubTestHttpClients
    {
        public static void SetupApiHttpClient(HttpClient httpClient)
        {
            if (httpClient is null)
            {
                throw new ArgumentNullException(nameof(httpClient));
            }

            httpClient.BaseAddress = new Uri("https://api.github.com");
            httpClient.DefaultRequestVersion = new Version(1, 0);
            httpClient.Timeout = TimeSpan.FromSeconds(30);
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Linux; Android 6.0; Nexus 5 Build/MRA58N) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.106 Mobile Safari/537.36");
            httpClient.MaxResponseContentBufferSize = HttpClientConstants.MaxResponseContentBufferSize;
        }

        public static void SetupRawHttpClient(HttpClient httpClient)
        {
            if (httpClient is null)
            {
                throw new ArgumentNullException(nameof(httpClient));
            }

            httpClient.BaseAddress = new Uri("https://raw.githubusercontent.com");
            httpClient.DefaultRequestVersion = new Version(1, 0);
            httpClient.Timeout = TimeSpan.FromSeconds(30);
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Linux; Android 6.0; Nexus 5 Build/MRA58N) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.106 Mobile Safari/537.36");
            httpClient.MaxResponseContentBufferSize = HttpClientConstants.MaxResponseContentBufferSize;
        }
    }
}