using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Atc.Helpers;
using AtcWeb.Domain.Data;
using AtcWeb.Domain.GitHub.Models;
using AtcWeb.Domain.Nuget;
using GitHubApiStatus;
using Octokit;

// ReSharper disable LoopCanBeConvertedToQuery
namespace AtcWeb.Domain.GitHub
{
    public class GitHubRepositoryService
    {
        private readonly GitHubApiClient gitHubApiClient;
        private readonly NugetApiClient nugetApiClient;

        public GitHubRepositoryService(
            GitHubApiClient gitHubApiClient,
            NugetApiClient nugetApiClient)
        {
            this.gitHubApiClient = gitHubApiClient ?? throw new ArgumentNullException(nameof(gitHubApiClient));
            this.nugetApiClient = nugetApiClient ?? throw new ArgumentNullException(nameof(nugetApiClient));
        }

        public async Task<GitHubApiRateLimits?> GetApiRateLimitsAsync()
        {
            var (isSuccessful, gitHubContributors) = await gitHubApiClient.GetAtcApiRateLimits();
            return isSuccessful
                ? gitHubContributors
                : null;
        }

        public async Task<List<RepositoryContributor>> GetContributorsAsync()
        {
            var (isSuccessful, gitHubContributors) = await gitHubApiClient.GetAtcContributors();
            return isSuccessful
                ? gitHubContributors
                : new List<RepositoryContributor>();
        }

        public async Task<List<RepositoryContributor>> GetResponsibleMembersAsGitHubContributor(string repositoryName)
        {
            var memberNames = RepositoryMetadata.GetResponsibleMembersByName(repositoryName);
            var gitHubContributors = await GetContributorsAsync();
            var data = new List<RepositoryContributor>();
            foreach (var memberName in memberNames.OrderBy(x => x))
            {
                var gitHubContributor =
                    gitHubContributors.Find(x => x.Login.Equals(memberName, StringComparison.OrdinalIgnoreCase));
                if (gitHubContributor is not null)
                {
                    data.Add(gitHubContributor);
                }
            }

            return data;
        }

        public async Task<List<AtcRepository>> GetRepositoriesAsync(bool populateMetaDataBase = false, bool populateMetaDataAdvanced = false)
        {
            var bag = new ConcurrentBag<AtcRepository>();
            var (isSuccessfulRepositories, repositories) = await gitHubApiClient.GetAtcRepositories();
            if (!isSuccessfulRepositories)
            {
                return bag.ToList();
            }

            var tasks = repositories
                .OrderBy(x => x.Name)
                .Select(async repository =>
                {
                    var atcRepository = new AtcRepository(repository);

                    if (populateMetaDataBase)
                    {
                        await PopulateMetaDataBase(atcRepository, repository);

                        if (populateMetaDataAdvanced)
                        {
                            await PopulateMetaDataAdvanced(atcRepository);
                        }
                    }

                    bag.Add(atcRepository);
                });

            await TaskHelper.WhenAll(tasks);

            return bag.ToList();
        }

        public async Task<AtcRepository?> GetRepositoryByNameAsync(string repositoryName, bool populateMetaDataBase = false, bool populateMetaDataAdvanced = false)
        {
            var (isSuccessful, repository) = await gitHubApiClient.GetAtcRepositoryByName(repositoryName);
            if (!isSuccessful || repository is null)
            {
                return null;
            }

            var atcRepository = new AtcRepository(repository);

            if (populateMetaDataBase)
            {
                await PopulateMetaDataBase(atcRepository, repository);

                if (populateMetaDataAdvanced)
                {
                    await PopulateMetaDataAdvanced(atcRepository);
                }
            }

            return atcRepository;
        }

        private async Task<List<GitHubPath>> GetDirectoryMetadata(string repositoryName, string defaultBranchName)
        {
            var (isSuccessful, gitHubPath) = await gitHubApiClient.GetAtcAllPathsByRepositoryByName(
                repositoryName,
                defaultBranchName);

            return isSuccessful
                ? gitHubPath
                : new List<GitHubPath>();
        }

        private async Task PopulateMetaDataBase(AtcRepository repository, Repository gitHubRepository)
        {
            repository.ResponsibleMembers = await GetResponsibleMembersAsGitHubContributor(repository.Name);

            repository.FolderAndFilePaths = await GetDirectoryMetadata(
                gitHubRepository.Name,
                repository.BaseData.DefaultBranch);

            var taskRoot = GitHubRepositoryMetadataHelper.LoadRoot(
                gitHubApiClient,
                repository.FolderAndFilePaths,
                repository.Name,
                repository.BaseData.DefaultBranch);

            var taskWorkflow = GitHubRepositoryMetadataHelper.LoadWorkflow(
                gitHubApiClient,
                repository.FolderAndFilePaths,
                repository.Name);

            var tasks = new List<Task>
            {
                taskRoot, taskWorkflow,
            };

            await TaskHelper.WhenAll(tasks);

            repository.Root = await taskRoot;
            repository.Workflow = await taskWorkflow;

            repository.SetBadges();
        }

        [SuppressMessage("Design", "MA0051:Method is too long", Justification = "OK.")]
        private async Task PopulateMetaDataAdvanced(AtcRepository repository)
        {
            var taskCodingRules = GitHubRepositoryMetadataHelper.LoadCodingRules(
                gitHubApiClient,
                repository.FolderAndFilePaths,
                repository.Name);

            var taskOpenIssues = GitHubRepositoryMetadataHelper.LoadOpenIssues(
                gitHubApiClient,
                repository.Name);

            var tasks = new List<Task>
            {
                taskCodingRules, taskOpenIssues,
            };

            Task<DotnetMetadata>? taskDotnet = null;
            Task<PythonMetadata>? taskPython = null;

            if ("C#".Equals(repository.BaseData.Language, StringComparison.Ordinal))
            {
                taskDotnet = GitHubRepositoryMetadataHelper.LoadDotnet(
                    gitHubApiClient,
                    repository.FolderAndFilePaths,
                    repository.Name);

                tasks.Add(taskDotnet);
            }
            else if ("Python".Equals(repository.BaseData.Language, StringComparison.Ordinal))
            {
                taskPython = GitHubRepositoryMetadataHelper.LoadPython(
                    gitHubApiClient,
                    repository.FolderAndFilePaths,
                    repository.Name);

                tasks.Add(taskPython);
            }

            await TaskHelper.WhenAll(tasks);

            repository.CodingRules = await taskCodingRules;
            repository.OpenIssues = await taskOpenIssues;

            if ("C#".Equals(repository.BaseData.Language, StringComparison.Ordinal))
            {
                repository.Dotnet = await taskDotnet!;
                foreach (var nugetPackageVersion in repository.Dotnet.Projects.SelectMany(x => x.PackageReferences))
                {
                    var (isSuccessful, latestVersion) = await nugetApiClient.GetVersionForPackageId(
                        nugetPackageVersion.PackageId,
                        CancellationToken.None);

                    if (isSuccessful)
                    {
                        nugetPackageVersion.NewestVersion = latestVersion;
                    }
                }
            }
            else if ("Python".Equals(repository.BaseData.Language, StringComparison.Ordinal))
            {
                repository.Python = await taskPython!;
            }

            repository.SetBadges();
        }
    }
}