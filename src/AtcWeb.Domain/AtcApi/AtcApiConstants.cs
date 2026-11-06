namespace AtcWeb.Domain.AtcApi;

/// <summary>
/// Base address of the ATC API. Defaults to production; local development overrides it
/// from wwwroot/appsettings.Development.json at startup.
/// </summary>
public static class AtcApiConstants
{
    public const string ProductionBaseAddress = "https://atcnet-api-prod-api-ca-01.greenhill-862ffb7c.swedencentral.azurecontainerapps.io";

    public static string BaseAddress { get; private set; } = ProductionBaseAddress;

    public static void Configure(string? baseAddress)
    {
        if (!string.IsNullOrWhiteSpace(baseAddress))
        {
            BaseAddress = baseAddress.TrimEnd('/');
        }
    }
}