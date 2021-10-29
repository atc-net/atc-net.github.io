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
namespace AtcWeb.Domain.Tests.Services
{
    public class GitHubApiIntegrationTests
    {
        [Theory, AutoNSubstituteData]
        public async Task GetAtcRepositories(
            [NotNull] [Frozen] IHttpClientFactory httpClientFactory,
            [Frozen] IMemoryCache memoryCache,
            [NotNull] HttpClient httpClient,
            CancellationToken cancellationToken)
        {
            // Arrange
            SetupHttpClient(httpClient);

            httpClientFactory
                .CreateClient(HttpClientConstants.GitHubApiClient)
                .Returns(httpClient);

            var gitHubApiClient = new GitHubApiClient(httpClientFactory, memoryCache);

            // Act
            var (isSuccessful, gitHubRepositories) = await gitHubApiClient.GetAtcRepositories(cancellationToken);

            // Assert
            Assert.True(isSuccessful);

            gitHubRepositories
                .Should()
                .NotBeEmpty()
                .And
                .HaveCountGreaterThan(1);
        }

        [Theory, AutoNSubstituteData]
        public async Task GetAtcContributors(
            [NotNull] [Frozen] IHttpClientFactory httpClientFactory,
            [Frozen] IMemoryCache memoryCache,
            [NotNull] HttpClient httpClient,
            CancellationToken cancellationToken)
        {
            // Arrange
            SetupHttpClient(httpClient);

            httpClientFactory
                .CreateClient(HttpClientConstants.GitHubApiClient)
                .Returns(httpClient);

            var gitHubApiClient = new GitHubApiClient(httpClientFactory, memoryCache);

            // Act
            var (isSuccessful, gitHubContributors) = await gitHubApiClient.GetAtcContributors(cancellationToken);

            // Assert
            Assert.True(isSuccessful);

            gitHubContributors
                .Should()
                .NotBeEmpty()
                .And
                .HaveCountGreaterThan(1);
        }

        [Theory, AutoNSubstituteData]
        public async Task GetAtcContributorsByRepository(
            [NotNull] [Frozen] IHttpClientFactory httpClientFactory,
            [Frozen] IMemoryCache memoryCache,
            [NotNull] HttpClient httpClient,
            CancellationToken cancellationToken)
        {
            // Arrange
            SetupHttpClient(httpClient);

            httpClientFactory
                .CreateClient(HttpClientConstants.GitHubApiClient)
                .Returns(httpClient);

            var gitHubApiClient = new GitHubApiClient(httpClientFactory, memoryCache);

            // Act
            var (isSuccessful, gitHubContributors) = await gitHubApiClient.GetAtcContributorsByRepository("atc", cancellationToken);

            // Assert
            Assert.True(isSuccessful);

            gitHubContributors
                .Should()
                .NotBeEmpty()
                .And
                .HaveCountGreaterThan(1);
        }

        [Theory, AutoNSubstituteData]
        public async Task GetAtcAllPathsByRepository(
            [NotNull][Frozen] IHttpClientFactory httpClientFactory,
            [Frozen] IMemoryCache memoryCache,
            [NotNull] HttpClient httpClient,
            CancellationToken cancellationToken)
        {
            // Arrange
            SetupHttpClient(httpClient);

            httpClientFactory
                .CreateClient(HttpClientConstants.GitHubApiClient)
                .Returns(httpClient);

            var gitHubApiClient = new GitHubApiClient(httpClientFactory, memoryCache);

            // Act
            var (isSuccessful, gitHubPaths) = await gitHubApiClient.GetAtcAllPathsByRepository("atc", "master", cancellationToken);

            // Assert
            Assert.True(isSuccessful);

            gitHubPaths
                .Should()
                .NotBeEmpty()
                .And
                .HaveCountGreaterThan(1);
        }

        private static void SetupHttpClient(HttpClient httpClient)
        {
            httpClient.BaseAddress = new Uri("https://api.github.com");
            httpClient.DefaultRequestVersion = new Version(1, 0);
            httpClient.Timeout = TimeSpan.FromSeconds(30);
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Linux; Android 6.0; Nexus 5 Build/MRA58N) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.106 Mobile Safari/537.36");
        }
    }
}
