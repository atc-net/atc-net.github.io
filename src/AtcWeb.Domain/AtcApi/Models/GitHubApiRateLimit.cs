using System;
using System.Diagnostics.CodeAnalysis;

namespace AtcWeb.Domain.AtcApi.Models;

[SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "OK.")]
public class GitHubApiRateLimit
{
    public DateTimeOffset RateLimitReset_TimeRemaining { get; set; }

    public int RateLimit { get; set; }

    public int RemainingRequestCount { get; set; }

    public int RateLimitReset_UnixEpochSeconds { get; set; }

    public DateTimeOffset RateLimitReset_DateTime { get; set; }
}