using System.Threading.Tasks;
using Atc.Test;
using AtcWeb.Domain.AtcApi;
using AutoFixture.Xunit2;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace AtcWeb.Domain.Tests.AtcApi
{
    public class AtcApiGitHubRepositoryClientTests
    {
        [Theory, AutoNSubstituteData]
        public async Task GetRepositories(
            [Frozen] IMemoryCache memoryCache)
        {
            // Arrange
            var client = new AtcApiGitHubRepositoryClient(memoryCache);

            // Act
            var (isSuccessful, gitHubRepositories) = await client.GetRepositories();

            // Assert
            Assert.True(isSuccessful);

            gitHubRepositories
                .Should()
                .NotBeEmpty()
                .And
                .HaveCountGreaterThan(1);
        }

        [Theory, AutoNSubstituteData]
        public async Task GetRepositoryByName(
            [Frozen] IMemoryCache memoryCache)
        {
            var client = new AtcApiGitHubRepositoryClient(memoryCache);

            // Act
            var (isSuccessful, gitHubRepository) = await client.GetRepositoryByName("atc");

            // Assert
            Assert.True(isSuccessful);

            gitHubRepository
                .Should()
                .NotBeNull();
        }

        [Theory, AutoNSubstituteData]
        public async Task GetContributors(
            [Frozen] IMemoryCache memoryCache)
        {
            // Arrange
            var client = new AtcApiGitHubRepositoryClient(memoryCache);

            // Act
            var (isSuccessful, gitHubContributors) = await client.GetContributors();

            // Assert
            Assert.True(isSuccessful);

            gitHubContributors
                .Should()
                .NotBeEmpty()
                .And
                .HaveCountGreaterThan(1);
        }

        [Theory, AutoNSubstituteData]
        public async Task GetContributorsByRepository(
            [Frozen] IMemoryCache memoryCache)
        {
            // Arrange
            var client = new AtcApiGitHubRepositoryClient(memoryCache);

            // Act
            var (isSuccessful, gitHubContributors) = await client.GetContributorsByRepositoryByName("atc");

            // Assert
            Assert.True(isSuccessful);

            gitHubContributors
                .Should()
                .NotBeEmpty()
                .And
                .HaveCountGreaterThan(1);
        }

        [Theory, AutoNSubstituteData]
        public async Task GetAllPathsByRepository(
            [Frozen] IMemoryCache memoryCache)
        {
            // Arrange
            var client = new AtcApiGitHubRepositoryClient(memoryCache);

            // Act
            var (isSuccessful, gitHubPaths) = await client.GetAllPathsByRepositoryByName("atc");

            // Assert
            Assert.True(isSuccessful);

            gitHubPaths
                .Should()
                .NotBeEmpty()
                .And
                .HaveCountGreaterThan(1);
        }

        [Theory, AutoNSubstituteData]
        public async Task GetFileByRepositoryNameAndFilePath(
            [Frozen] IMemoryCache memoryCache)
        {
            // Arrange
            var client = new AtcApiGitHubRepositoryClient(memoryCache);

            // Act
            var (isSuccessful, gitHubRaw) = await client.GetFileByRepositoryNameAndFilePath("atc", "README.md");

            // Assert
            Assert.True(isSuccessful);

            gitHubRaw
                .Should()
                .NotBeEmpty();
        }

        [Theory, AutoNSubstituteData]
        public async Task GetIssuesAllByRepositoryByName(
            [Frozen] IMemoryCache memoryCache)
        {
            // Arrange
            var client = new AtcApiGitHubRepositoryClient(memoryCache);

            // Act
            var (isSuccessful, gitHubIssues) = await client.GetIssuesAllByRepositoryByName("atc");

            // Assert
            Assert.True(isSuccessful);

            gitHubIssues
                .Should()
                .NotBeEmpty()
                .And
                .HaveCountGreaterThan(1);
        }

        [Theory, AutoNSubstituteData]
        public async Task GetIssuesOpenByRepositoryByName(
            [Frozen] IMemoryCache memoryCache)
        {
            // Arrange
            var client = new AtcApiGitHubRepositoryClient(memoryCache);

            // Act
            var (isSuccessful, gitHubIssues) = await client.GetIssuesOpenByRepositoryByName("atc");

            // Assert
            Assert.True(isSuccessful);

            gitHubIssues
                .Should()
                .NotBeEmpty()
                .And
                .HaveCountGreaterThan(1);
        }

        [Theory, AutoNSubstituteData]
        public async Task GetIssuesClosedByRepositoryByName(
            [Frozen] IMemoryCache memoryCache)
        {
            // Arrange
            var client = new AtcApiGitHubRepositoryClient(memoryCache);

            // Act
            var (isSuccessful, gitHubIssues) = await client.GetIssuesClosedByRepositoryByName("atc");

            // Assert
            Assert.True(isSuccessful);

            gitHubIssues
                .Should()
                .NotBeEmpty()
                .And
                .HaveCountGreaterThan(1);
        }
    }
}