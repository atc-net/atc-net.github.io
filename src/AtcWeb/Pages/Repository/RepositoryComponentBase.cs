using System.Threading.Tasks;
using AtcWeb.Domain.GitHub;
using Microsoft.AspNetCore.Components;

namespace AtcWeb.Pages.Repository
{
    public class RepositoryComponentBase : ComponentBase
    {
        private readonly string repositoryName;
        protected Domain.GitHub.Models.Repository? repository;

        [Inject]
        protected GitHubRepositoryService RepositoryService { get; set; }

        protected RepositoryComponentBase(string repositoryName)
        {
            this.repositoryName = repositoryName;
        }

        protected override async Task OnInitializedAsync()
        {
            repository = await RepositoryService.GetRepositoryByNameAsync(repositoryName, true);
            await base.OnInitializedAsync();
        }
    }
}