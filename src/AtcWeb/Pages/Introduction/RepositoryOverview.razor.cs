using System.Collections.Generic;
using System.Threading.Tasks;
using AtcWeb.Domain.GitHub;
using Microsoft.AspNetCore.Components;

namespace AtcWeb.Pages.Introduction
{
    public class RepositoryOverviewBase : ComponentBase
    {
        protected List<Domain.GitHub.Models.Repository> repositories;

        [Inject]
        protected GitHubRepositoryService RepositoryService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            repositories = await RepositoryService.GetRepositoriesAsync();

            await base.OnInitializedAsync();
        }
    }
}