using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

// ReSharper disable InvertIf
namespace AtcWeb.Domain.GitHub.Models
{
    public class Repository
    {
        public Repository(GitHubRepository gitHubRepository)
        {
            this.BaseData = gitHubRepository ?? throw new ArgumentNullException(nameof(gitHubRepository));
            this.DefaultBranchName = "main";
            if (gitHubRepository.Name.Equals("atc", StringComparison.Ordinal) ||
                gitHubRepository.Name.Equals("atc-autoformatter", StringComparison.Ordinal))
            {
                this.DefaultBranchName = "master";
            }

            this.Badges = new List<(string Group, string Key, Uri Url)>
            {
                ("General Project Info", "Github top language", new Uri($"https://img.shields.io/github/languages/top/atc-net/{gitHubRepository.Name}")),
                ("General Project Info", "Github stars", new Uri($"https://img.shields.io/github/stars/atc-net/{gitHubRepository.Name}")),
                ("General Project Info", "Github forks", new Uri($"https://img.shields.io/github/forks/atc-net/{gitHubRepository.Name}")),
                ("General Project Info", "Github size", new Uri($"https://img.shields.io/github/repo-size/atc-net/{gitHubRepository.Name}")),
                ("General Project Info", "Issues Open", new Uri($"https://img.shields.io/github/issues/atc-net/{gitHubRepository.Name}.svg?logo=github")),
            };

            FoldersAndFiles = new DirectoryItem();
            Root = new RootMetadata();
            Workflow = new WorkflowMetadata();
            CodingRules = new CodingRulesMetadata();
            Dotnet = new DotnetMetadata();
        }

        public GitHubRepository BaseData { get; }

        public string Name => BaseData.Name;

        public string DefaultBranchName { get; }

        public List<(string Group, string Key, Uri Url)> Badges { get; }

        public string Description { get; private set; }

        public DirectoryItem FoldersAndFiles { get; set; }

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
                    new Uri($"https://github.com/atc-net/{BaseData.Name}/workflows/Pre-Integration/badge.svg")));
            }

            if (HasWorkflowPostIntegration)
            {
                Badges.Add((
                    "Build Status",
                    "Post-Integration",
                    new Uri($"https://github.com/atc-net/{BaseData.Name}/workflows/Post-Integration/badge.svg")));
            }

            if (HasWorkflowRelease)
            {
                Badges.Add((
                    "Build Status",
                    "Release",
                    new Uri($"https://github.com/atc-net/{BaseData.Name}/workflows/Release/badge.svg")));
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