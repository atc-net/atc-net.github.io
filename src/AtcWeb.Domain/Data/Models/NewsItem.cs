namespace AtcWeb.Domain.Data.Models;

public record struct NewsItem(DateTimeOffset Time, NewsItemAction Action, string RepositoryName, string Title, string Body);