using System.Collections.Generic;
using System.Threading.Tasks;
using AtcWeb.Domain.GitHub;
using AtcWeb.Domain.GitHub.Models;
using Microsoft.AspNetCore.Components;

namespace AtcWeb.Pages.Introduction
{
    public class TeamAndContributorsBase : ComponentBase
    {
        protected List<GitHubContributor> contributors;

        [Inject]
        protected GitHubRepositoryService RepositoryService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            contributors = await RepositoryService.GetContributorsAsync();

            await base.OnInitializedAsync();
        }
    }
}