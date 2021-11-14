using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AtcWeb.Domain.GitHub;
using AtcWeb.Extensions;
using AtcWeb.Models;
using Microsoft.AspNetCore.Components;

namespace AtcWeb.Shared
{
    public partial class NavMenu
    {
        private string? section;
        private string? componentLink;

        protected List<Domain.GitHub.Models.AtcRepository>? repositories;

        [Inject]
        private NavigationManager NavigationManager { get; set; }

        [Inject]
        protected GitHubRepositoryService RepositoryService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            repositories = await RepositoryService.GetRepositoriesAsync();

            Refresh();
            await base.OnInitializedAsync();
        }

        public void Refresh()
        {
            section = NavigationManager.GetSection();
            componentLink = NavigationManager.GetComponentLink();
            StateHasChanged();
        }

        public bool IsSubGroupExpanded(AtcComponent? item)
            => item is not null &&
               item.GroupItems.Elements.Any(i => i.Link == componentLink);
    }
}