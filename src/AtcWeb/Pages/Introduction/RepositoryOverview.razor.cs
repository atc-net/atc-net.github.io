using System.Collections.Generic;
using System.Threading.Tasks;
using AtcWeb.Domain.GitHub;
using Microsoft.AspNetCore.Components;

namespace AtcWeb.Pages.Introduction
{
    public class RepositoryOverviewBase : ComponentBase
    {
        protected List<Domain.GitHub.Models.AtcRepository>? Repositories;

        [Inject]
        protected GitHubRepositoryService RepositoryService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            Repositories = await RepositoryService.GetRepositoriesAsync(populateMetaDataBase: true, populateMetaDataAdvanced: false);

            await base.OnInitializedAsync();
        }
    }
}