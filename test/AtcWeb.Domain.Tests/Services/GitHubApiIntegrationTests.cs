using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Atc.Test;
using AtcWeb.Domain.GitHub.Clients;
using AutoFixture.Xunit2;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace AtcWeb.Domain.Tests.Services
{
    public class GitHubApiIntegrationTests
    {
        [Theory, AutoNSubstituteData]
        public async Task GetAtcRepositories(
            [Frozen] IHttpClientFactory httpClientFactory,
            [Frozen] IMemoryCache memoryCache,
            CancellationToken cancellationToken)
        {
            // Arrange
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
            [Frozen] IHttpClientFactory httpClientFactory,
            [Frozen] IMemoryCache memoryCache,
            CancellationToken cancellationToken)
        {
            // Arrange
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
            [Frozen] IHttpClientFactory httpClientFactory,
            [Frozen] IMemoryCache memoryCache,
            CancellationToken cancellationToken)
        {
            // Arrange
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
        public async Task GetRootPaths(
            [Frozen] IHttpClientFactory httpClientFactory,
            [Frozen] IMemoryCache memoryCache,
            CancellationToken cancellationToken)
        {
            // Arrange
            var gitHubApiClient = new GitHubApiClient(httpClientFactory, memoryCache);

            // Act
            var (isSuccessful, gitHubPaths) = await gitHubApiClient.GetRootPaths("atc", cancellationToken);

            // Assert
            Assert.True(isSuccessful);

            gitHubPaths
                .Should()
                .NotBeEmpty()
                .And
                .HaveCountGreaterThan(1);
        }

        [Theory, AutoNSubstituteData]
        public async Task GetTreePaths(
            [Frozen] IHttpClientFactory httpClientFactory,
            [Frozen] IMemoryCache memoryCache,
            CancellationToken cancellationToken)
        {
            // Arrange
            var gitHubApiClient = new GitHubApiClient(httpClientFactory, memoryCache);

            // Act
            var (isSuccessful, gitHubPaths) = await gitHubApiClient.GetTreePaths("https://api.github.com/repos/atc-net/atc/git/trees/3bed1d2b6788adc65eb4255605fc8783aa9818ff", cancellationToken);

            // Assert
            Assert.True(isSuccessful);

            gitHubPaths
                .Should()
                .NotBeEmpty()
                .And
                .HaveCountGreaterThan(1);
        }
    }
}