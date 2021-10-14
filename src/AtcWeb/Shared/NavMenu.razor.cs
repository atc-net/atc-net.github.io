using System.Linq;
using AtcWeb.Extensions;
using AtcWeb.Models;
using Microsoft.AspNetCore.Components;

namespace AtcWeb.Shared
{
    public partial class NavMenu
    {
        [Inject] NavigationManager NavMan { get; set; }

        string section;
        string componentLink;

        protected override void OnInitialized()
        {
            Refresh();
            base.OnInitialized();
        }

        public void Refresh()
        {
            section = NavMan.GetSection();
            componentLink = NavMan.GetComponentLink();
            StateHasChanged();
        }

        bool IsSubGroupExpanded(AtcComponent item)
        {
            return item.GroupItems.Elements.Any(i => i.Link == componentLink);
        }
    }
}