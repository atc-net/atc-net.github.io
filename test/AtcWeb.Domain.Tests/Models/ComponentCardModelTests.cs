namespace AtcWeb.Domain.Tests.Models;

public sealed class ComponentCardModelTests
{
    [Fact]
    public void From_Library_UsesFirstPackageForInstallCommand()
    {
        List<RepositoryNugetPackage> packages =
        [
            new("Atc.Cosmos", "2.0.0", "Cosmos", 100, IsTool: false),
            new("Atc.Cosmos.Testing", "2.0.0", "Testing", 50, IsTool: false),
        ];

        var model = ComponentCardModel.From(NewRepository("atc-cosmos"), packages);

        model.InstallCommand.Should().Be("dotnet add package Atc.Cosmos");
        model.Downloads.Should().Be(150);
        model.IsCliTool.Should().BeFalse();
        model.HasPackage.Should().BeTrue();
    }

    [Fact]
    public void From_Tool_UsesToolInstallCommand()
    {
        var model = ComponentCardModel.From(
            NewRepository("atc-coding-rules-updater"),
            [new RepositoryNugetPackage("atc-coding-rules-updater", "2.0.0", "Tool", 10, IsTool: true)]);

        model.InstallCommand.Should().Be("dotnet tool install -g atc-coding-rules-updater");
        model.IsCliTool.Should().BeTrue();
        model.HasCliTool.Should().BeTrue();
        model.HasLibrary.Should().BeFalse();
    }

    [Fact]
    public void From_LibraryAndTool_IsBothLibraryAndCliTool()
    {
        List<RepositoryNugetPackage> packages =
        [
            new("Atc.Azure.IoT", "1.0.0", "Library", 100, IsTool: false),
            new("atc-azure-iot", "1.0.0", "Tool", 10, IsTool: true),
        ];

        var model = ComponentCardModel.From(NewRepository("atc-azure-iot"), packages);

        model.InstallCommand.Should().Be("dotnet add package Atc.Azure.IoT");
        model.IsCliTool.Should().BeFalse();
        model.HasLibrary.Should().BeTrue();
        model.HasCliTool.Should().BeTrue();
        model.LibraryInstallCommand.Should().Be("dotnet add package Atc.Azure.IoT");
        model.ToolInstallCommand.Should().Be("dotnet tool install -g atc-azure-iot");
    }

    [Fact]
    public void From_NoPackages_HasNoInstallCommand()
    {
        var model = ComponentCardModel.From(NewRepository("atc-docs"), []);

        model.InstallCommand.Should().BeNull();
        model.HasPackage.Should().BeFalse();
        model.Downloads.Should().Be(0);
    }

    private static AtcRepository NewRepository(string name)
        => new(new GitHubRepository
        {
            Name = name,
            Description = $"{name} description",
            Language = "C#",
            PushedAt = DateTimeOffset.UtcNow,
        });
}