using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AtcWeb.Domain.GitHub.Models
{
    public class Repository
    {
        public Repository(GitHubRepository gitHubRepository)
        {
            this.BaseData = gitHubRepository ?? throw new ArgumentNullException();
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
        }

        public GitHubRepository BaseData { get; }

        public string DefaultBranchName { get; }

        public string GetDotName()
            => BaseData.Name
                .PascalCase()
                .Replace("-", ".")
                .Replace("Autoformatter", "AutoFormatter", StringComparison.Ordinal);

        public List<(string Group, string Key, Uri Url)> Badges { get; }

        public string Description { get; private set; }

        public WorkflowMetadata Workflow { get; private set; }

        public DotnetMetadata Dotnet { get; private set; }

        public bool HasWorkflowPreIntegration => Workflow?.HasPreIntegration ?? false;

        public bool HasWorkflowPostIntegration => Workflow?.HasPostIntegration ?? false;

        public bool HasWorkflowRelease => Workflow?.HasRelease ?? false;

        public bool HasDotnetSolutionFile => Dotnet?.HasSolutionFile ?? false;

        public async Task Load(GitHubApiClient gitHubApiClient, CancellationToken cancellationToken)
        {
            Description = "Coming soon...";
            Workflow = await LoadWorkflow(gitHubApiClient, cancellationToken);
            Dotnet = await LoadDotnet(gitHubApiClient, cancellationToken);

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

            // TODO: IF
            if (HasWorkflowPreIntegration)
            {
                Badges.Add((
                    "Packages",
                    "Github Version",
                    new Uri("https://img.shields.io/static/v1?logo=github&color=blue&label=github&message=latest")));
                
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

        private async Task<WorkflowMetadata> LoadWorkflow(GitHubApiClient gitHubApiClient, CancellationToken cancellationToken)
        {
            var data = new WorkflowMetadata();

            (bool isSuccessfulPreIntegration, string rawPreIntegration) = await gitHubApiClient.GetRawAtcWorkflowFile(BaseData.Name, DefaultBranchName, "pre-integration.yml", cancellationToken);
            if (isSuccessfulPreIntegration)
            {
                data.RawPreIntegration = rawPreIntegration;

                (bool isSuccessfulPostIntegration, string rawPostIntegration) = await gitHubApiClient.GetRawAtcWorkflowFile(BaseData.Name, DefaultBranchName, "post-integration.yml", cancellationToken);
                if (isSuccessfulPostIntegration)
                {
                    data.RawPostIntegration = rawPostIntegration;
                }

                (bool isSuccessfulRelease, string rawRelease) = await gitHubApiClient.GetRawAtcWorkflowFile(BaseData.Name, DefaultBranchName, "release.yml", cancellationToken);
                if (isSuccessfulRelease)
                {
                    data.RawRelease = rawRelease;
                }
            }

            return data;
        }

        private async Task<DotnetMetadata> LoadDotnet(GitHubApiClient gitHubApiClient, CancellationToken cancellationToken)
        {
            var data = new DotnetMetadata();

            (bool isSuccessfulSolutionFile, string rawSolutionFile) = await gitHubApiClient.GetRawAtcSolutionFile(BaseData.Name, GetDotName() + ".sln", DefaultBranchName, cancellationToken);
            if (isSuccessfulSolutionFile)
            {
                data.RawSolutionFile = rawSolutionFile;
            }

            return data;
        }
    }
}