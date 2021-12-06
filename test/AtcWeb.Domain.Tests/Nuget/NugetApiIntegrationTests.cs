using System;
using System.Threading;
using System.Threading.Tasks;
using Atc.Test;
using AtcWeb.Domain.Nuget;
using AutoFixture.Xunit2;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace AtcWeb.Domain.Tests.Nuget
{
    public class NugetApiIntegrationTests
    {
        [Theory, AutoNSubstituteData]
        public async Task GetAtcVersion(
            [Frozen] IMemoryCache memoryCache)
        {
            // Arrange
            var nugetApiClient = new NugetApiClient(memoryCache);
            const string packageId = "Atc";

            // Act
            var (isSuccessful, version) = await nugetApiClient.GetVersionForPackageId(packageId, CancellationToken.None);

            // Assert
            Assert.True(isSuccessful);
            Assert.True(version >= new Version());
        }
    }
}