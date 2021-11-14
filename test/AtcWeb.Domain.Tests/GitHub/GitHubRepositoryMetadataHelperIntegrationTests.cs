using System.Collections.Generic;
using System.Threading.Tasks;
using Atc.Test;
using AtcWeb.Domain.GitHub;
using AtcWeb.Domain.GitHub.Models;
using AutoFixture.Xunit2;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

// ReSharper disable RedundantNullableFlowAttribute
namespace AtcWeb.Domain.Tests.GitHub
{
    public class GitHubRepositoryMetadataHelperIntegrationTests
    {
        [Theory, AutoNSubstituteData]
        public async Task LoadRoot(
            [Frozen] IMemoryCache memoryCache)
        {
            // Arrange
            var gitHubClient = GitHubTestHttpClients.CreateGitHubClient();
            var gitHubApiClient = new GitHubApiClient(gitHubClient, memoryCache);
            var filesAndFolders = new List<GitHubPath>
            {
                new GitHubPath
                {
                    Type = "blob",
                    Path = "README.md",
                },
            };

            // Act
            var actual = await GitHubRepositoryMetadataHelper.LoadRoot(gitHubApiClient, filesAndFolders, "atc", "master");

            // Assert
            actual
                .Should()
                .NotBeNull();
        }
    }
}