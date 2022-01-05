using System.Collections.Generic;
using System.Threading.Tasks;
using AtcWeb.Domain.AtcApi.Models;
using AtcWeb.Domain.GitHub;
using Microsoft.AspNetCore.Components;

namespace AtcWeb.Pages.Introduction
{
    public class TeamAndContributorsBase : ComponentBase
    {
        protected List<GitHubRepositoryContributor>? Contributors { get; set; }

        [Inject]
        protected GitHubRepositoryService RepositoryService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            Contributors = await RepositoryService.GetContributorsAsync();

            await base.OnInitializedAsync();
        }
    }
}