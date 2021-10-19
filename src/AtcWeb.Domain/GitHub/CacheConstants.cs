using System;

namespace AtcWeb.Domain.GitHub
{
    public static class CacheConstants
    {
        public const string CacheKeyRepositories = "Repositories";

        public const string CacheKeyContributors = "Contributors";

        public static TimeSpan SlidingExpiration = TimeSpan.FromHours(6);

        public static TimeSpan AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(12);
    }
}