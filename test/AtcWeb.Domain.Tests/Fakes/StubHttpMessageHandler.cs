namespace AtcWeb.Domain.Tests.Fakes;

/// <summary>
/// Returns canned responses per request URL (path + query), so API clients can be tested without the network.
/// Unknown URLs return 404.
/// </summary>
public sealed class StubHttpMessageHandler : HttpMessageHandler
{
    private readonly Dictionary<string, (HttpStatusCode StatusCode, string Content)> responses = new(StringComparer.Ordinal);

    public void Add(
        string pathAndQuery,
        string content,
        HttpStatusCode statusCode = HttpStatusCode.OK)
        => responses[pathAndQuery] = (statusCode, content);

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var key = request.RequestUri!.PathAndQuery;
        var response = responses.TryGetValue(key, out var canned)
            ? new HttpResponseMessage(canned.StatusCode) { Content = new StringContent(canned.Content) }
            : new HttpResponseMessage(HttpStatusCode.NotFound);

        return Task.FromResult(response);
    }
}