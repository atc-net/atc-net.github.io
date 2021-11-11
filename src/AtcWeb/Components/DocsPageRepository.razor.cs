using AtcWeb.Domain.GitHub.Models;
using Microsoft.AspNetCore.Components;

namespace AtcWeb.Components
{
    public class DocsPageRepositoryBase : ComponentBase
    {
        [Parameter]
        public Repository? Repository { get; set; }

        [Parameter]
        public RenderFragment ChildContent { get; set; }
    }
}