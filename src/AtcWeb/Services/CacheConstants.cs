using System;

namespace AtcWeb.Services
{
    public static class CacheConstants
    {
        public const string CacheKeyRepositories = "Repositories";

        public const string CacheKeyContributorsAll = "ContributorsAll";

        public static TimeSpan SlidingExpiration = TimeSpan.FromHours(6);

        public static TimeSpan AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(12);
    }
}