using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AtcWeb.Domain.Data;
using AtcWeb.Domain.GitHub.Models;
using GitHubApiStatus;
using Octokit;

// ReSharper disable LoopCanBeConvertedToQuery
namespace AtcWeb.Domain.GitHub
{
    public class GitHubRepositoryService
    {
        private readonly GitHubApiClient gitHubApiClient;

        public GitHubRepositoryService(GitHubApiClient gitHubApiClient)
        {
            this.gitHubApiClient = gitHubApiClient ?? throw new ArgumentNullException(nameof(gitHubApiClient));
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
                var gitHubContributor = gitHubContributors.Find(x => x.Login.Equals(memberName, StringComparison.OrdinalIgnoreCase));
                if (gitHubContributor is not null)
                {
                    data.Add(gitHubContributor);
                }
            }

            return data;
        }

        public async Task<List<AtcRepository>> GetRepositoriesAsync(bool populateMetaData = false)
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

                if (populateMetaData)
                {
                    await PopulateMetaData(atcRepository, repository);
                }

                bag.Add(atcRepository);
            });

            // TODO: ATC-WhenAll
            await Task.WhenAll(tasks);

            return bag.ToList();
        }

        public async Task<AtcRepository?> GetRepositoryByNameAsync(string repositoryName, bool populateMetaData = false)
        {
            var (isSuccessful, repository) = await gitHubApiClient.GetAtcRepositoryByName(repositoryName);
            if (!isSuccessful || repository is null)
            {
                return null;
            }

            var atcRepository = new AtcRepository(repository);

            if (populateMetaData)
            {
                await PopulateMetaData(atcRepository, repository);
            }

            return atcRepository;
        }

        private async Task<List<GitHubPath>> GetDirectoryMetadata(string repositoryName, string defaultBranchName)
        {
            var (isSuccessful, gitHubPath) = await gitHubApiClient.GetAtcAllPathsByRepositoryByName(repositoryName, defaultBranchName);
            return isSuccessful
                ? gitHubPath
                : new List<GitHubPath>();
        }

        private async Task PopulateMetaData(AtcRepository repository, Repository gitHubRepository)
        {
            repository.ResponsibleMembers = await GetResponsibleMembersAsGitHubContributor(repository.Name);

            repository.FolderAndFilePaths = await GetDirectoryMetadata(
                gitHubRepository.Name,
                repository.DefaultBranchName);

            var taskRoot = GitHubRepositoryMetadataHelper.LoadRoot(
                gitHubApiClient,
                repository.FolderAndFilePaths,
                repository.Name,
                repository.DefaultBranchName);

            var taskWorkflow = GitHubRepositoryMetadataHelper.LoadWorkflow(
                gitHubApiClient,
                repository.FolderAndFilePaths,
                repository.Name);

            var taskCodingRules = GitHubRepositoryMetadataHelper.LoadCodingRules(
                gitHubApiClient,
                repository.FolderAndFilePaths,
                repository.Name);

            var taskDotnet = GitHubRepositoryMetadataHelper.LoadDotnet(
                gitHubApiClient,
                repository.FolderAndFilePaths,
                repository.Name);

            var tasks = new List<Task>
            {
                taskRoot,
                taskWorkflow,
                taskCodingRules,
                taskDotnet,
            };

            // TODO: ATC-WhenAll
            await Task.WhenAll(tasks);

            repository.Root = await taskRoot;
            repository.Workflow = await taskWorkflow;
            repository.CodingRules = await taskCodingRules;
            repository.Dotnet = await taskDotnet;

            repository.SetBadges();
        }
    }
}