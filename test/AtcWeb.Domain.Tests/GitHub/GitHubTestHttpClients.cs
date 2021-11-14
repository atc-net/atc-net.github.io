using System;
using Octokit;

namespace AtcWeb.Domain.Tests.GitHub
{
    public static class GitHubTestHttpClients
    {
        public static GitHubClient CreateGitHubClient()
        {
            var tokenAuth = new Credentials(HttpClientConstants.AtcAccessToken.Base64Decode());
            var gitHubClient = new GitHubClient(new ProductHeaderValue(HttpClientConstants.AtcOrganizationName))
            {
                Credentials = tokenAuth,
            };

            return gitHubClient;
        }
    }
}