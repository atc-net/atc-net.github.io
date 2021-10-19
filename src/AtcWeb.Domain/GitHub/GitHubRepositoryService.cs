using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AtcWeb.Domain.GitHub.Models;

namespace AtcWeb.Domain.GitHub
{
    public class GitHubRepositoryService
    {
        private readonly GitHubApiClient gitHubApiClient;

        public GitHubRepositoryService(GitHubApiClient gitHubApiClient)
        {
            this.gitHubApiClient = gitHubApiClient ?? throw new ArgumentNullException(nameof(gitHubApiClient));
        }

        public async Task<List<Repository>> GetRepositoriesAsync(CancellationToken cancellationToken = default)
        {
            var data = new List<Repository>();
            var endpointResult = await gitHubApiClient.GetAtcRepositories(cancellationToken);
            if (endpointResult.isSuccessful)
            {
                foreach (var gitHubRepository in endpointResult.Item2.OrderBy(x => x.Name))
                {
                    var repository = new Repository(gitHubRepository);
                    await repository.Load(gitHubApiClient, cancellationToken);
                    data.Add(repository);
                }
            }

            return data;
        }
    }
}