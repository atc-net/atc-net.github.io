using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Atc;
using Octokit;

// ReSharper disable InvertIf
namespace AtcWeb.Domain.GitHub.Models
{
    public class AtcRepository
    {
        public AtcRepository(Repository repository)
        {
            this.BaseData = repository ?? throw new ArgumentNullException(nameof(repository));
            this.DefaultBranchName = "main";
            if (repository.Name.Equals("atc", StringComparison.Ordinal))
            {
                this.DefaultBranchName = "master";
            }

            Badges = new List<(string Group, string Key, Uri Url)>();
            FolderAndFilePaths = new List<GitHubPath>();
            Root = new RootMetadata();
            Workflow = new WorkflowMetadata();
            CodingRules = new CodingRulesMetadata();
            Dotnet = new DotnetMetadata();
        }

        public Repository BaseData { get; }

        public string Name => BaseData.Name;

        public string DotName => Name
            .Replace('-', '.')
            .Replace("atc.rest.api.generator", "atc-api-gen", StringComparison.Ordinal)
            .Replace("atc.coding.rules.updater", "atc-coding-rules-updater", StringComparison.Ordinal);

        public string Description => BaseData.Description;

        public string Url => $"https://github.com/atc-net/{Name}";

        public string DefaultBranchName { get; }

        public List<(string Group, string Key, Uri Url)> Badges { get; private set; }

        public List<RepositoryContributor> ResponsibleMembers { get; set; }

        public List<GitHubPath> FolderAndFilePaths { get; set; }

        public RootMetadata Root { get; set; }

        public WorkflowMetadata Workflow { get; set; }

        public CodingRulesMetadata CodingRules { get; set; }

        public DotnetMetadata Dotnet { get; set; }

        public List<Issue> OpenIssues { get; set; }

        public bool HasRootReadme => Root?.HasReadme ?? false;

        public bool HasWorkflowPreIntegration => Workflow?.HasPreIntegration ?? false;

        public bool HasWorkflowPostIntegration => Workflow?.HasPostIntegration ?? false;

        public bool HasWorkflowRelease => Workflow?.HasRelease ?? false;

        public bool HasCodingRulesEditorConfigRoot => CodingRules?.HasRoot ?? false;

        public bool HasCodingRulesEditorConfigSrc => CodingRules?.HasSrc ?? false;

        public bool HasCodingRulesEditorConfigTest => CodingRules?.HasTest ?? false;

        public bool HasDotnetSolution => Dotnet?.HasSolution ?? false;

        public bool HasDotnetDirectoryBuildPropsRoot => Dotnet?.HasDirectoryBuildPropsRoot ?? false;

        public bool HasDotnetDirectoryBuildPropsSrc => Dotnet?.HasDirectoryBuildPropsSrc ?? false;

        public bool HasDotnetDirectoryBuildPropsTest => Dotnet?.HasDirectoryBuildPropsTest ?? false;

        [SuppressMessage("Design", "MA0051:Method is too long", Justification = "OK.")]
        public void SetBadges()
        {
            Badges = new List<(string Group, string Key, Uri Url)>
            {
                ("General Project Info", "Github top language", new Uri($"https://img.shields.io/github/languages/top/atc-net/{Name}?logo=github")),
                ("General Project Info", "Github stars", new Uri($"https://img.shields.io/github/stars/atc-net/{Name}?logo=github&label=Stars")),
                ("General Project Info", "Github watchers", new Uri($"https://img.shields.io/github/watchers/atc-net/{Name}?logo=github&label=Watch")),
                ("General Project Info", "Github forks", new Uri($"https://img.shields.io/github/forks/atc-net/{Name}?logo=github&label=Forks")),
                ("General Project Info", "Github size", new Uri($"https://img.shields.io/github/repo-size/atc-net/{Name}?logo=github&label=Repo-size")),
                ("General Project Info", "Issues Open", new Uri($"https://img.shields.io/github/issues/atc-net/{Name}.svg?logo=github&label=Issues")),
            };

            if (HasWorkflowPreIntegration)
            {
                Badges.Add((
                    "Build Status",
                    "Pre-Integration",
                    new Uri($"{Url}/workflows/Pre-Integration/badge.svg")));
            }

            if (HasWorkflowPostIntegration)
            {
                Badges.Add((
                    "Build Status",
                    "Post-Integration",
                    new Uri($"{Url}/workflows/Post-Integration/badge.svg")));
            }

            if (HasWorkflowRelease)
            {
                Badges.Add((
                    "Build Status",
                    "Release",
                    new Uri($"{Url}/workflows/Release/badge.svg")));
            }

            if (HasWorkflowPostIntegration && HasDotnetSolution)
            {
                Badges.Add((
                    "Packages",
                    "Github Version",
                    new Uri("https://img.shields.io/static/v1?logo=github&color=blue&label=GitHub&message=latest")));
            }

            if (HasWorkflowRelease && HasDotnetSolution)
            {
                Badges.Add((
                    "Packages",
                    "NuGet Version",
                    new Uri($"https://img.shields.io/nuget/v/{DotName}.svg?logo=nuget&label=Nuget")));

                Badges.Add((
                    "Packages",
                    "Downloads",
                    new Uri($"https://img.shields.io/nuget/dt/{DotName}?logo=nuget&style=flat-square&label=Downloads")));

                Badges.Add((
                    "Code Quality",
                    "Maintainability Rating",
                    new Uri($"https://sonarcloud.io/api/project_badges/measure?project={Name}&metric=sqale_rating")));

                Badges.Add((
                    "Code Quality",
                    "Reliability Rating",
                    new Uri($"https://sonarcloud.io/api/project_badges/measure?project={Name}&metric=reliability_rating")));

                Badges.Add((
                    "Code Quality",
                    "Security Rating",
                    new Uri($"https://sonarcloud.io/api/project_badges/measure?project={Name}&metric=security_rating")));

                Badges.Add((
                    "Code Quality",
                    "Bugs",
                    new Uri($"https://sonarcloud.io/api/project_badges/measure?project={Name}&metric=bugs")));

                Badges.Add((
                    "Code Quality",
                    "Vulnerabilities",
                    new Uri($"https://sonarcloud.io/api/project_badges/measure?project={Name}&metric=vulnerabilities")));
            }
        }

        public DateTimeOffset? GetOpenIssuesNewest()
        {
            if (OpenIssues.Count == 0)
            {
                return null;
            }

            return OpenIssues.Max(x => x.CreatedAt);
        }

        public DateTimeOffset? GetOpenIssuesOldest()
        {
            if (OpenIssues.Count == 0)
            {
                return null;
            }

            return OpenIssues.Min(x => x.CreatedAt);
        }

        public LogCategoryType GetOpenIssuesNewestState(int monthWarning, int monthError) => GetOpenIssuesState(GetOpenIssuesNewest(), monthWarning, monthError);

        public LogCategoryType GetOpenIssuesOldestState(int monthWarning, int monthError) => GetOpenIssuesState(GetOpenIssuesOldest(), monthWarning, monthError);

        private static LogCategoryType GetOpenIssuesState(DateTimeOffset? date, int monthWarning, int monthError)
        {
            var logCategoryType = LogCategoryType.Information;
            if (date is not null)
            {
                if (date.Value <= DateTimeOffset.Now.AddMonths(monthWarning * -1))
                {
                    logCategoryType = LogCategoryType.Warning;
                }

                if (date.Value <= DateTimeOffset.Now.AddMonths(monthError * -1))
                {
                    logCategoryType = LogCategoryType.Error;
                }
            }

            return logCategoryType;
        }
    }
}