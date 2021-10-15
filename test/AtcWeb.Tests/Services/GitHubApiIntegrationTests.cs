using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Atc.Test;
using AtcWeb.Services;
using AutoFixture.Xunit2;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace AtcWeb.Tests.Services
{
    public class GitHubApiIntegrationTests
    {
        [Theory, AutoNSubstituteData]
        public async Task Can_Get_Repositories(
            [Frozen] IMemoryCache memoryCache,
            CancellationToken cancellationToken)
        {
            // Arrange
            var gitHubApiClient = new GitHubApiClient(new HttpClient(), memoryCache);

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
        public async Task Can_Get_Contributors(
            [Frozen] IMemoryCache memoryCache,
            CancellationToken cancellationToken)
        {
            // Arrange
            var gitHubApiClient = new GitHubApiClient(new HttpClient(), memoryCache);

            // Act
            var (isSuccessful, gitHubContributors) = await gitHubApiClient.GetAllAtcContributors(cancellationToken);

            // Assert
            Assert.True(isSuccessful);

            gitHubContributors
                .Should()
                .NotBeEmpty()
                .And
                .HaveCountGreaterThan(1);
        }

        [Theory, AutoNSubstituteData]
        public async Task Can_Get_Contributors_For_Single_Repository(
            [Frozen] IMemoryCache memoryCache,
            CancellationToken cancellationToken)
        {
            // Arrange
            var gitHubApiClient = new GitHubApiClient(new HttpClient(), memoryCache);

            // Act
            var (isSuccessful, gitHubContributors) = await gitHubApiClient.GetContributorsByRepository("atc", cancellationToken);

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