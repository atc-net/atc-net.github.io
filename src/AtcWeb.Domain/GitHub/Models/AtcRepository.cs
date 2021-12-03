using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

            this.Badges = new List<(string Group, string Key, Uri Url)>
            {
                ("General Project Info", "Github top language", new Uri($"https://img.shields.io/github/languages/top/atc-net/{repository.Name}")),
                ("General Project Info", "Github stars", new Uri($"https://img.shields.io/github/stars/atc-net/{repository.Name}")),
                ("General Project Info", "Github forks", new Uri($"https://img.shields.io/github/forks/atc-net/{repository.Name}")),
                ("General Project Info", "Github size", new Uri($"https://img.shields.io/github/repo-size/atc-net/{repository.Name}")),
                ("General Project Info", "Issues Open", new Uri($"https://img.shields.io/github/issues/atc-net/{repository.Name}.svg?logo=github")),
            };

            FolderAndFilePaths = new List<GitHubPath>();
            Root = new RootMetadata();
            Workflow = new WorkflowMetadata();
            CodingRules = new CodingRulesMetadata();
            Dotnet = new DotnetMetadata();
        }

        public Repository BaseData { get; }

        public string Name => BaseData.Name;

        public string Description => BaseData.Description;

        public string Url => $"https://github.com/atc-net/{Name}";

        public string DefaultBranchName { get; }

        public List<(string Group, string Key, Uri Url)> Badges { get; }

        public List<RepositoryContributor> ResponsibleMembers { get; set; }

        public List<GitHubPath> FolderAndFilePaths { get; set; }

        public RootMetadata Root { get; set; }

        public WorkflowMetadata Workflow { get; set; }

        public CodingRulesMetadata CodingRules { get; set; }

        public DotnetMetadata Dotnet { get; set; }

        public bool HasRootReadme => Root?.HasReadme ?? false;

        public bool HasWorkflowPreIntegration => Workflow?.HasPreIntegration ?? false;

        public bool HasWorkflowPostIntegration => Workflow?.HasPostIntegration ?? false;

        public bool HasWorkflowRelease => Workflow?.HasRelease ?? false;

        public bool HasCodingRulesEditorConfigRoot => CodingRules?.HasEditorConfigRoot ?? false;

        public bool HasCodingRulesEditorConfigSrc => CodingRules?.HasEditorConfigSrc ?? false;

        public bool HasCodingRulesEditorConfigTest => CodingRules?.HasEditorConfigTest ?? false;

        public bool HasDotnetSolution => Dotnet?.HasSolution ?? false;

        public bool HasDotnetDirectoryBuildPropsRoot => Dotnet?.HasDirectoryBuildPropsRoot ?? false;

        public bool HasDotnetDirectoryBuildPropsSrc => Dotnet?.HasDirectoryBuildPropsSrc ?? false;

        public bool HasDotnetDirectoryBuildPropsTest => Dotnet?.HasDirectoryBuildPropsTest ?? false;

        [SuppressMessage("Design", "MA0051:Method is too long", Justification = "OK.")]
        public void SetBadges()
        {
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
                    new Uri("https://img.shields.io/static/v1?logo=github&color=blue&label=github&message=latest")));
            }

            if (HasWorkflowRelease && HasDotnetSolution)
            {
                Badges.Add((
                    "Packages",
                    "NuGet Version",
                    new Uri($"https://img.shields.io/nuget/v/{BaseData.Name}.svg?logo=nuget")));

                Badges.Add((
                    "Code Quality",
                    "Maintainability Rating",
                    new Uri($"https://sonarcloud.io/api/project_badges/measure?project={BaseData.Name}&metric=sqale_rating")));

                Badges.Add((
                    "Code Quality",
                    "Reliability Rating",
                    new Uri($"https://sonarcloud.io/api/project_badges/measure?project={BaseData.Name}&metric=reliability_rating")));

                Badges.Add((
                    "Code Quality",
                    "Security Rating",
                    new Uri($"https://sonarcloud.io/api/project_badges/measure?project={BaseData.Name}&metric=security_rating")));

                Badges.Add((
                    "Code Quality",
                    "Bugs",
                    new Uri($"https://sonarcloud.io/api/project_badges/measure?project={BaseData.Name}&metric=bugs")));

                Badges.Add((
                    "Code Quality",
                    "Vulnerabilities",
                    new Uri($"https://sonarcloud.io/api/project_badges/measure?project={BaseData.Name}&metric=vulnerabilities")));
            }
        }
    }
}