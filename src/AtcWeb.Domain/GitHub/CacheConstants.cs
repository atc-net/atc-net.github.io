using System;

namespace AtcWeb.Domain.GitHub
{
    public static class CacheConstants
    {
        public const string CacheKeyRepositories = "Repositories";

        public const string CacheKeyContributors = "Contributors";

        public const string CacheKeyCodeFile = "CodeFile";

        public const string CacheKeyNugetSearchQuery = "NugetSearchQuery";

        public const string CacheKeyNugetPackageId = "NugetPackageId";

        public static readonly TimeSpan SlidingExpiration = TimeSpan.FromHours(6);

        public static readonly TimeSpan AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(12);
    }
}