using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Atc.Serialization;
using AtcWeb.Domain.GitHub;
using AtcWeb.Domain.Nuget.Models;
using Microsoft.Extensions.Caching.Memory;

namespace AtcWeb.Domain.Nuget
{
    public class NugetApiClient
    {
        private readonly IMemoryCache memoryCache;

        public NugetApiClient(IMemoryCache memoryCache)
        {
            this.memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        }

        public Task<(bool isSuccessful, Version)> GetVersionForPackageId(string packageId, CancellationToken cancellationToken)
        {
            if (packageId is null)
            {
                throw new ArgumentNullException(nameof(packageId));
            }

            return InvokeGetVersionForPackageId(packageId, cancellationToken);
        }

        public Task<(bool isSuccessful, NugetSearchResult)> Search(string query, CancellationToken cancellationToken)
        {
            if (query is null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            return InvokeSearch(query, cancellationToken);
        }

        private async Task<(bool isSuccessful, Version)> InvokeGetVersionForPackageId(string packageId, CancellationToken cancellationToken)
        {
            var cacheKey = $"{CacheConstants.CacheKeyNugetPackageId}_{packageId}";
            if (memoryCache.TryGetValue(cacheKey, out Version data))
            {
                return (isSuccessful: true, data);
            }

            var sa = packageId.Split('.', StringSplitOptions.RemoveEmptyEntries);
            var query = sa.First();
            if (sa.Length > 1 && "Microsoft".Equals(query, StringComparison.Ordinal))
            {
                query = sa.Length == 3
                    ? $"{sa[0]}.{sa[1]}.{sa[2]}"
                    : $"{sa[0]}.{sa[1]}";
            }

            var (isSuccessful, nugetSearchResult) = await Search(query, cancellationToken);
            if (!isSuccessful)
            {
                return (isSuccessful: false, new Version());
            }

            var package = nugetSearchResult.Data.Find(x => x.Id.Equals(packageId, StringComparison.OrdinalIgnoreCase));
            if (package is null)
            {
                return (isSuccessful: false, new Version());
            }

            if (!Version.TryParse(package.Version, out var result))
            {
                return (isSuccessful: false, new Version());
            }

            memoryCache.Set(cacheKey, nugetSearchResult, CacheConstants.AbsoluteExpirationRelativeToNow);
            return (isSuccessful: true, result);
        }

        private async Task<(bool isSuccessful, NugetSearchResult)> InvokeSearch(string query, CancellationToken cancellationToken)
        {
            try
            {
                var cacheKey = $"{CacheConstants.CacheKeyNugetSearchQuery}_{query}";
                if (memoryCache.TryGetValue(cacheKey, out NugetSearchResult data))
                {
                    return (isSuccessful: true, data);
                }

                using var httpClient = new HttpClient
                {
                    BaseAddress = new Uri("https://azuresearch-usnc.nuget.org"),
                };

                var responseMessage = await httpClient.GetAsync($"/query?q={query}&take=1000&prerelease=false", cancellationToken);
                if (!responseMessage.IsSuccessStatusCode)
                {
                    return (isSuccessful: false, new NugetSearchResult());
                }

                var content = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
                var nugetSearchResult = JsonSerializer.Deserialize<NugetSearchResult>(content, JsonSerializerOptionsFactory.Create());
                if (nugetSearchResult is null)
                {
                    return (isSuccessful: false, new NugetSearchResult());
                }

                memoryCache.Set(cacheKey, nugetSearchResult, CacheConstants.AbsoluteExpirationRelativeToNow);
                return (isSuccessful: true, nugetSearchResult);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}