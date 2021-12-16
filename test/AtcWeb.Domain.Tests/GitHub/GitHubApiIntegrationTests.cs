using System.Threading.Tasks;
using Atc.Test;
using AtcWeb.Domain.GitHub;
using AutoFixture.Xunit2;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

// ReSharper disable RedundantNullableFlowAttribute
namespace AtcWeb.Domain.Tests.GitHub
{
    public class GitHubApiIntegrationTests
    {
        [Theory, AutoNSubstituteData]
        public async Task GetAtcRepositories(
            [Frozen] IMemoryCache memoryCache)
        {
            // Arrange
            var gitHubClient = GitHubTestHttpClients.CreateGitHubClient();
            var gitHubApiClient = new GitHubApiClient(gitHubClient, memoryCache);

            // Act
            var (isSuccessful, gitHubRepositories) = await gitHubApiClient.GetAtcRepositories();

            // Assert
            Assert.True(isSuccessful);

            gitHubRepositories
                .Should()
                .NotBeEmpty()
                .And
                .HaveCountGreaterThan(1);
        }

        [Theory, AutoNSubstituteData]
        public async Task GetAtcRepositoryByName(
            [Frozen] IMemoryCache memoryCache)
        {
            // Arrange
            var gitHubClient = GitHubTestHttpClients.CreateGitHubClient();
            var gitHubApiClient = new GitHubApiClient(gitHubClient, memoryCache);

            // Act
            var (isSuccessful, gitHubRepository) = await gitHubApiClient.GetAtcRepositoryByName("atc");

            // Assert
            Assert.True(isSuccessful);

            gitHubRepository
                .Should()
                .NotBeNull();
        }

        [Theory, AutoNSubstituteData]
        public async Task GetAtcContributors(
            [Frozen] IMemoryCache memoryCache)
        {
            // Arrange
            var gitHubClient = GitHubTestHttpClients.CreateGitHubClient();
            var gitHubApiClient = new GitHubApiClient(gitHubClient, memoryCache);

            // Act
            var (isSuccessful, gitHubContributors) = await gitHubApiClient.GetAtcContributors();

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
            [Frozen] IMemoryCache memoryCache)
        {
            // Arrange
            var gitHubClient = GitHubTestHttpClients.CreateGitHubClient();
            var gitHubApiClient = new GitHubApiClient(gitHubClient, memoryCache);

            // Act
            var (isSuccessful, gitHubContributors) = await gitHubApiClient.GetAtcContributorsByRepositoryByName("atc");

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
            [Frozen] IMemoryCache memoryCache)
        {
            // Arrange
            var gitHubClient = GitHubTestHttpClients.CreateGitHubClient();
            var gitHubApiClient = new GitHubApiClient(gitHubClient, memoryCache);

            // Act
            var (isSuccessful, gitHubPaths) = await gitHubApiClient.GetAtcAllPathsByRepositoryByName("atc", "main");

            // Assert
            Assert.True(isSuccessful);

            gitHubPaths
                .Should()
                .NotBeEmpty()
                .And
                .HaveCountGreaterThan(1);
        }

        [Theory, AutoNSubstituteData]
        public async Task GetRawAtcCodeFile(
            [Frozen] IMemoryCache memoryCache)
        {
            // Arrange
            var gitHubClient = GitHubTestHttpClients.CreateGitHubClient();
            var gitHubApiClient = new GitHubApiClient(gitHubClient, memoryCache);

            // Act
            var (isSuccessful, gitHubRaw) = await gitHubApiClient.GetRawAtcCodeFile("atc", "README.md");

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
            var gitHubClient = GitHubTestHttpClients.CreateGitHubClient();
            var gitHubApiClient = new GitHubApiClient(gitHubClient, memoryCache);

            // Act
            var (isSuccessful, gitHubIssues) = await gitHubApiClient.GetIssuesAllByRepositoryByName("atc");

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
            var gitHubClient = GitHubTestHttpClients.CreateGitHubClient();
            var gitHubApiClient = new GitHubApiClient(gitHubClient, memoryCache);

            // Act
            var (isSuccessful, gitHubIssues) = await gitHubApiClient.GetIssuesOpenByRepositoryByName("atc");

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
            var gitHubClient = GitHubTestHttpClients.CreateGitHubClient();
            var gitHubApiClient = new GitHubApiClient(gitHubClient, memoryCache);

            // Act
            var (isSuccessful, gitHubIssues) = await gitHubApiClient.GetIssuesClosedByRepositoryByName("atc");

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