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
    }
}