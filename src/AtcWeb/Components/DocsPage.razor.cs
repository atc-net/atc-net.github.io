using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AtcWeb.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AtcWeb.Components
{
    public partial class DocsPage : ComponentBase
    {
        private readonly Queue<DocsSectionLink> bufferedSections = new ();
        private MudPageContentNavigation? contentNavigation;

        [Inject]
        private NavigationManager NavigationManager { get; set; }

        [Parameter]
        public MaxWidth MaxWidth { get; set; } = MaxWidth.Medium;

        [Parameter]
        public RenderFragment? ChildContent { get; set; }

        private bool contentDrawerOpen = true;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender && contentNavigation is not null)
            {
                await contentNavigation.ScrollToSection(new Uri(NavigationManager.Uri));
            }
        }

        internal void AddSection(DocsSectionLink section)
        {
            bufferedSections.Enqueue(section);

            if (contentNavigation is null)
            {
                return;
            }

            while (bufferedSections.Count > 0)
            {
                var item = bufferedSections.Dequeue();

                if (contentNavigation.Sections.FirstOrDefault(x => x.Id == section.Id) == default)
                {
                    contentNavigation.AddSection(item.Title, item.Id, forceUpdate: false);
                }
            }

            contentNavigation.Update();
        }
    }
}