using System.Threading.Tasks;
using AtcWeb.Domain.GitHub;
using GitHubApiStatus;
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
            RateLimits = await RepositoryService.GetApiRateLimitsAsync();

            await base.OnInitializedAsync();
        }
    }
}