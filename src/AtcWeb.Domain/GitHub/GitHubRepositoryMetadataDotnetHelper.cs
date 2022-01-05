using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AtcWeb.Domain.AtcApi;
using AtcWeb.Domain.AtcApi.Models;
using AtcWeb.Domain.GitHub.Models;

// ReSharper disable LoopCanBeConvertedToQuery
// ReSharper disable UseObjectOrCollectionInitializer
namespace AtcWeb.Domain.GitHub
{
    public static class GitHubRepositoryMetadataDotnetHelper
    {
        [SuppressMessage("Design", "MA0051:Method is too long", Justification = "OK.")]
        public static async Task<List<DotnetProject>> GetProjects(
            AtcApiGitHubRepositoryClient gitHubRepositoryClient,
            List<GitHubPath> foldersAndFiles,
            string repositoryName,
            string rawDirectoryBuildPropsRoot,
            string rawDirectoryBuildPropsSrc,
            string rawDirectoryBuildPropsTest)
        {
            var data = new List<DotnetProject>();
            var gitHubCsprojPaths = foldersAndFiles
                .Where(x => x.IsFile && x.Path.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
                .OrderBy(x => x.Path)
                .ToList();

            foreach (var gitHubCsprojPath in gitHubCsprojPaths)
            {
                var project = new DotnetProject();

                project.RawCsproj = await GitHubRepositoryMetadataFileHelper.GetFileByPath(
                    gitHubRepositoryClient,
                    foldersAndFiles,
                    repositoryName,
                    gitHubCsprojPath.Path);

                if (string.IsNullOrEmpty(project.RawCsproj))
                {
                    continue;
                }

                project.Name = gitHubCsprojPath
                    .GetFileName()
                    .Replace(".csproj", string.Empty, StringComparison.Ordinal);

                project.CompilerSettings.TargetFramework = GetSimpleXmlValueForCsproj(
                    gitHubCsprojPath.Path,
                    project.RawCsproj,
                    rawDirectoryBuildPropsRoot,
                    rawDirectoryBuildPropsSrc,
                    rawDirectoryBuildPropsTest,
                    "TargetFramework");

                project.CompilerSettings.LangVersion = GetSimpleXmlValueForCsproj(
                    gitHubCsprojPath.Path,
                    project.RawCsproj,
                    rawDirectoryBuildPropsRoot,
                    rawDirectoryBuildPropsSrc,
                    rawDirectoryBuildPropsTest,
                    "LangVersion");

                project.CompilerSettings.GenerateDocumentationFile = "true".Equals(
                    GetSimpleXmlValueForCsproj(
                        gitHubCsprojPath.Path,
                        project.RawCsproj,
                        rawDirectoryBuildPropsRoot,
                        rawDirectoryBuildPropsSrc,
                        rawDirectoryBuildPropsTest,
                        "GenerateDocumentationFile"),
                    StringComparison.OrdinalIgnoreCase);

                project.CompilerSettings.Type = GetProjectTypeForCsproj(
                    gitHubCsprojPath.Path,
                    project.RawCsproj);

                project.AnalyzerSettings.AnalysisMode = GetSimpleXmlValueForCsproj(
                    gitHubCsprojPath.Path,
                    project.RawCsproj,
                    rawDirectoryBuildPropsRoot,
                    rawDirectoryBuildPropsSrc,
                    rawDirectoryBuildPropsTest,
                    "AnalysisMode");

                project.AnalyzerSettings.AnalysisLevel = GetSimpleXmlValueForCsproj(
                    gitHubCsprojPath.Path,
                    project.RawCsproj,
                    rawDirectoryBuildPropsRoot,
                    rawDirectoryBuildPropsSrc,
                    rawDirectoryBuildPropsTest,
                    "AnalysisLevel");

                project.AnalyzerSettings.AnalysisLevel = GetSimpleXmlValueForCsproj(
                    gitHubCsprojPath.Path,
                    project.RawCsproj,
                    rawDirectoryBuildPropsRoot,
                    rawDirectoryBuildPropsSrc,
                    rawDirectoryBuildPropsTest,
                    "AnalysisLevel");

                project.AnalyzerSettings.EnableNetAnalyzers = "true".Equals(
                    GetSimpleXmlValueForCsproj(
                        gitHubCsprojPath.Path,
                        project.RawCsproj,
                        rawDirectoryBuildPropsRoot,
                        rawDirectoryBuildPropsSrc,
                        rawDirectoryBuildPropsTest,
                        "EnableNETAnalyzers"),
                    StringComparison.OrdinalIgnoreCase);

                project.AnalyzerSettings.EnforceCodeStyleInBuild = "true".Equals(
                    GetSimpleXmlValueForCsproj(
                        gitHubCsprojPath.Path,
                        project.RawCsproj,
                        rawDirectoryBuildPropsRoot,
                        rawDirectoryBuildPropsSrc,
                        rawDirectoryBuildPropsTest,
                        "EnforceCodeStyleInBuild"),
                    StringComparison.OrdinalIgnoreCase);

                project.IsPackage = !string.IsNullOrEmpty(
                    GetSimpleXmlValueForCsproj(
                        gitHubCsprojPath.Path,
                        project.RawCsproj,
                        "PackageId"));

                project.PackageReferences = GetAllPackageReferencesForCsproj(
                    gitHubCsprojPath.Path,
                    project.RawCsproj,
                    rawDirectoryBuildPropsRoot,
                    rawDirectoryBuildPropsSrc,
                    rawDirectoryBuildPropsTest);

                data.Add(project);
            }

            return data;
        }

        private static string GetProjectTypeForCsproj(
            string filePath,
            string rawCsproj)
        {
            var outputType = GetSimpleXmlValueForCsproj(
                filePath,
                rawCsproj,
                "OutputType");

            if ("exe".Equals(outputType, StringComparison.OrdinalIgnoreCase))
            {
                return "CLI";
            }

            var useWpf = GetSimpleXmlValueForCsproj(
                filePath,
                rawCsproj,
                "UseWPF");

            if ("true".Equals(useWpf, StringComparison.OrdinalIgnoreCase))
            {
                return "winexe".Equals(outputType, StringComparison.OrdinalIgnoreCase)
                    ? "WPF Application"
                    : "WPF Library";
            }

            if (rawCsproj.Contains("Sdk=\"Microsoft.NET.Sdk.BlazorWebAssembly\"", StringComparison.Ordinal))
            {
                return "Blazor WebAssembly";
            }

            if (rawCsproj.Contains("Sdk=\"Microsoft.NET.Sdk.Web\"", StringComparison.Ordinal))
            {
                return "Web";
            }

            if (rawCsproj.Contains("Sdk=\"Microsoft.NET.Sdk.Razor\"", StringComparison.Ordinal))
            {
                return "Web Library";
            }

            return "Library";
        }

        private static List<DotnetNugetPackageVersionExtended> GetAllPackageReferencesForCsproj(
            string filePath,
            string projectRawCsproj,
            string rawDirectoryBuildPropsRoot,
            string rawDirectoryBuildPropsSrc,
            string rawDirectoryBuildPropsTest)
        {
            var data = new List<DotnetNugetPackageVersionExtended>();

            data.AddRange(GetPackageReferencesForCsproj(projectRawCsproj));

            if (string.IsNullOrEmpty(rawDirectoryBuildPropsSrc) &&
                filePath.StartsWith("src", StringComparison.Ordinal))
            {
                data.AddRange(GetPackageReferencesForCsproj(rawDirectoryBuildPropsSrc));
            }
            else if (string.IsNullOrEmpty(rawDirectoryBuildPropsTest) &&
                     filePath.StartsWith("test", StringComparison.Ordinal))
            {
                data.AddRange(GetPackageReferencesForCsproj(rawDirectoryBuildPropsTest));
            }

            if (!string.IsNullOrEmpty(rawDirectoryBuildPropsRoot))
            {
                data.AddRange(GetPackageReferencesForCsproj(rawDirectoryBuildPropsRoot));
            }

            return data
                .OrderBy(x => x.PackageId)
                .ToList();
        }

        private static IEnumerable<DotnetNugetPackageVersionExtended> GetPackageReferencesForCsproj(string rawCsproj)
        {
            var data = new List<DotnetNugetPackageVersionExtended>();
            foreach (var line in rawCsproj.EnsureEnvironmentNewLines().Split(Environment.NewLine))
            {
                if (!line.Contains("<PackageReference ", StringComparison.Ordinal) ||
                    !line.Contains("Include=", StringComparison.Ordinal) ||
                    !line.Contains("Version=", StringComparison.Ordinal))
                {
                    continue;
                }

                var attributes = line
                    .Replace("<PackageReference ", string.Empty, StringComparison.Ordinal)
                    .Replace("/>", string.Empty, StringComparison.Ordinal)
                    .Replace(">", string.Empty, StringComparison.Ordinal)
                    .Trim()
                    .Split(' ');

                var packageId = string.Empty;
                var version = string.Empty;

                foreach (var attribute in attributes)
                {
                    if (attribute.StartsWith("Include=", StringComparison.Ordinal))
                    {
                        packageId = attribute
                            .Replace("Include=", string.Empty, StringComparison.Ordinal)
                            .Replace("\"", string.Empty, StringComparison.Ordinal);
                    }
                    else if (attribute.StartsWith("Version=", StringComparison.Ordinal))
                    {
                        version = attribute
                            .Replace("Version=", string.Empty, StringComparison.Ordinal)
                            .Replace("\"", string.Empty, StringComparison.Ordinal);
                    }
                }

                if (!string.IsNullOrEmpty(packageId) &&
                    !string.IsNullOrEmpty(version) &&
                    Version.TryParse(version, out var dotnetVersion))
                {
                    data.Add(new DotnetNugetPackageVersionExtended(packageId, dotnetVersion, dotnetVersion));
                }
            }

            return data.OrderBy(x => x.PackageId);
        }

        private static string GetSimpleXmlValueForCsproj(
            string filePath,
            string rawCsproj,
            string xmlElement)
        {
            return GetSimpleXmlValueForCsproj(
                filePath,
                rawCsproj,
                string.Empty,
                string.Empty,
                string.Empty,
                xmlElement);
        }

        private static string GetSimpleXmlValueForCsproj(
            string filePath,
            string rawCsproj,
            string rawDirectoryBuildPropsRoot,
            string rawDirectoryBuildPropsSrc,
            string rawDirectoryBuildPropsTest,
            string xmlElement)
        {
            var value = ExtractSimpleXmlElementValue(rawCsproj, xmlElement);
            if (!string.IsNullOrEmpty(value))
            {
                return value;
            }

            if (string.IsNullOrEmpty(rawDirectoryBuildPropsSrc) &&
                filePath.StartsWith("src", StringComparison.Ordinal))
            {
                value = ExtractSimpleXmlElementValue(rawDirectoryBuildPropsSrc, xmlElement);
            }
            else if (string.IsNullOrEmpty(rawDirectoryBuildPropsTest) &&
                     filePath.StartsWith("test", StringComparison.Ordinal))
            {
                value = ExtractSimpleXmlElementValue(rawDirectoryBuildPropsTest, xmlElement);
            }

            if (!string.IsNullOrEmpty(value))
            {
                return value;
            }

            return string.IsNullOrEmpty(rawDirectoryBuildPropsRoot)
                ? string.Empty
                : ExtractSimpleXmlElementValue(rawDirectoryBuildPropsRoot, xmlElement);
        }

        private static string ExtractSimpleXmlElementValue(string input, string xmlElement)
        {
            var match = Regex.Match(
                input,
                $"<({xmlElement})>.*?</\\1",
                RegexOptions.Compiled | RegexOptions.ECMAScript,
                TimeSpan.FromSeconds(1));

            if (match.Groups.Count != 2)
            {
                return string.Empty;
            }

            var xmlElementWithValue = match.Groups[0].Value;
            return xmlElementWithValue
                .Replace($"<{xmlElement}>", string.Empty, StringComparison.Ordinal)
                .Replace($"</{xmlElement}", string.Empty, StringComparison.Ordinal);
        }
    }
}