// Local development host for the site.
//
// "atc-api-url" picks the API the site talks to (chosen in the dashboard, or preset with
// --Parameters:atc-api-url=<url> / user secrets):
//   LOCAL    - https://localhost:51612/; run atc-api yourself (e.g. its own AppHost).
//   DEPLOYED - the deployed atc-api.
//
// The site is a standalone Blazor WebAssembly app: its configuration is read in the browser, so environment variables
// set on a resource never reach it. The URL is therefore baked into the build ("dotnet run -p:AtcApiBaseAddress=<url>",
// see AtcWeb.csproj), which is why the site is started with "dotnet run" rather than AddProject (which runs --no-build).
// That build goes to its own artifacts folder, so a normal build (IDE, dotnet build/test) can't overwrite the running
// site's files with ones that lack the URL.
#pragma warning disable ASPIREINTERACTION001
var builder = DistributedApplication.CreateBuilder(args);

const string localAtcApiUrl = "https://localhost:51612/";
const string deployedAtcApiUrl = "https://atcnet-api-prod-api-ca-01.greenhill-862ffb7c.swedencentral.azurecontainerapps.io/";

var atcApiUrl = builder
    .AddParameter("atc-api-url")
    .WithDescription("The URL of atc-api")
    .WithCustomInput(_ => new InteractionInput
    {
        Name = "AtcApiUrl",
        InputType = InputType.Choice,
        Label = "atc-api Target Environment",
        Options =
        [
            KeyValuePair.Create(localAtcApiUrl, "LOCAL"),
            KeyValuePair.Create(deployedAtcApiUrl, "DEPLOYED"),
        ],
    })
    .WithInitialState(new CustomResourceSnapshot
    {
        ResourceType = "CustomHiddenParameterType",
        Properties = [],
        IsHidden = true,
    });

var atcApi = builder
    .AddExternalService("atc-api", atcApiUrl)
    .WithHttpHealthCheck("/health");

var webProjectPath = Path.GetFullPath("../AtcWeb/AtcWeb.csproj", builder.AppHostDirectory);
var webArtifactsPath = Path.GetFullPath("../../artifacts/apphost-web", builder.AppHostDirectory);

builder
    .AddExecutable(
        "web",
        "dotnet",
        builder.AppHostDirectory,
        "run",
        "--project",
        webProjectPath,
        "--launch-profile",
        "AtcWeb",
        "--artifacts-path",
        webArtifactsPath)
    .WithArgs(ReferenceExpression.Create($"-p:AtcApiBaseAddress={atcApiUrl}"))
    .WithReference(atcApi)
    .WithChildRelationship(atcApi)
    .WithUrl("https://localhost:45365", "Site")
    .WaitFor(atcApi);

await builder
    .Build()
    .RunAsync();