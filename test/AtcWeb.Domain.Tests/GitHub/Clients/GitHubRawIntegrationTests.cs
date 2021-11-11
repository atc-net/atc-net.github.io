using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Atc.Test;
using AtcWeb.Domain.GitHub.Clients;
using AutoFixture.Xunit2;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using NSubstitute;
using Xunit;

// ReSharper disable RedundantNullableFlowAttribute
namespace AtcWeb.Domain.Tests.GitHub.Clients
{
    public class GitHubRawIntegrationTests
    {
        [Theory, AutoNSubstituteData]
        public async Task GetRawAtcCodeFile(
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

            // Act
            var (isSuccessful, gitHubRaw) = await gitHubRawClient.GetRawAtcCodeFile("atc", "master", "README.md", cancellationToken);

            // Assert
            Assert.True(isSuccessful);

            gitHubRaw
                .Should()
                .NotBeEmpty();
        }
    }
}