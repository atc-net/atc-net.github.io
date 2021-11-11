using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Atc.Test;
using AtcWeb.Domain.GitHub;
using AtcWeb.Domain.GitHub.Clients;
using AtcWeb.Domain.GitHub.Models;
using AutoFixture.Xunit2;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using NSubstitute;
using Xunit;

// ReSharper disable RedundantNullableFlowAttribute
namespace AtcWeb.Domain.Tests.GitHub
{
    public class GitHubRepositoryMetadataHelperIntegrationTests
    {
        [Theory, AutoNSubstituteData]
        public async Task LoadRoot(
            [NotNull][Frozen] IHttpClientFactory httpClientFactory,
            [Frozen] IMemoryCache memoryCache,
            [NotNull] HttpClient httpClient,
            CancellationToken cancellationToken)
        {
            // Arrange
            GitHubTestHttpClients.SetupRawHttpClient(httpClient);

            httpClientFactory
                .CreateClient(HttpClientConstants.GitHubRawClient)
                .Returns(httpClient);

            var gitHubRawClient = new GitHubRawClient(httpClientFactory, memoryCache);
            var filesAndFolders = new List<GitHubPath>
            {
                new GitHubPath
                {
                    Type = "blob",
                    Path = "README.md",
                },
            };

            // Act
            var actual = await GitHubRepositoryMetadataHelper.LoadRoot(gitHubRawClient, filesAndFolders, "atc", "master", cancellationToken);

            // Assert
            actual
                .Should()
                .NotBeNull();
        }
    }
}