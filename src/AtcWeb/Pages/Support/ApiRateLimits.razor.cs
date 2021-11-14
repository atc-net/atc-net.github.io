using System.Threading.Tasks;
using AtcWeb.Domain.GitHub;
using GitHubApiStatus;
using Microsoft.AspNetCore.Components;

namespace AtcWeb.Pages.Support
{
    public class ApiRateLimitsBase : ComponentBase
    {
        protected GitHubApiRateLimits? rateLimits;

        [Inject]
        protected GitHubRepositoryService RepositoryService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            rateLimits = await RepositoryService.GetApiRateLimitsAsync();

            await base.OnInitializedAsync();
        }
    }
}