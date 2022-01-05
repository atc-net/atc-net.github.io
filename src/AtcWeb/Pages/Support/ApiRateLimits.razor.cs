using System;
using System.Threading.Tasks;
using AtcWeb.Domain.AtcApi.Models;
using AtcWeb.Domain.GitHub;
using Microsoft.AspNetCore.Components;

namespace AtcWeb.Pages.Support
{
    public class ApiRateLimitsBase : ComponentBase
    {
        protected GitHubApiRateLimits? RateLimits;

        [Inject]
        protected GitHubRepositoryService RepositoryService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            RateLimits = await RepositoryService.GetRestApiRateLimitsAsync();

            await base.OnInitializedAsync();
        }
    }
}