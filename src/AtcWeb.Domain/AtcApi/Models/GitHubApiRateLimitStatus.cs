// ReSharper disable InconsistentNaming
namespace AtcWeb.Domain.AtcApi.Models;

[SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "OK.")]
public class GitHubApiRateLimitStatus
{
    public TimeSpan RateLimitReset_TimeRemaining { get; set; }

    public int RateLimit { get; set; }

    public int RemainingRequestCount { get; set; }

    public long RateLimitReset_UnixEpochSeconds { get; set; }

    public DateTimeOffset RateLimitReset_DateTime { get; set; }

    public override string ToString() => $"{nameof(RateLimitReset_TimeRemaining)}: {RateLimitReset_TimeRemaining}, {nameof(RateLimit)}: {RateLimit}, {nameof(RemainingRequestCount)}: {RemainingRequestCount}, {nameof(RateLimitReset_UnixEpochSeconds)}: {RateLimitReset_UnixEpochSeconds}, {nameof(RateLimitReset_DateTime)}: {RateLimitReset_DateTime}";
}