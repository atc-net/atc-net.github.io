using System;
using System.Collections.Generic;
using System.Linq;

namespace AtcWeb.Models
{
    public class DocsComponents
    {
        private List<MudComponent> mudComponents = new();

        /// <summary>
        /// The elements of the list of mud-components
        /// </summary>
        internal IEnumerable<MudComponent> Elements => mudComponents.OrderBy(e => e.Name);

        public DocsComponents AddItem(string name, Type component, params Type[] childComponents)
        {
            var componentItem = new MudComponent
            {
                Name = name,
                Link = name.ToLower().Replace(" ", ""),
                Component = component,
                ChildComponents = childComponents,
                IsNavGroup = false
            };

            mudComponents.Add(componentItem);

            return this;
        }

        public DocsComponents AddNavGroup(string name, bool expanded, DocsComponents groupItems)
        {
            var componentItem = new MudComponent
            {
                Name = name,
                NavGroupExpanded = expanded,
                GroupItems = groupItems,
                IsNavGroup = true
            };

            mudComponents.Add(componentItem);

            return this;
        }
    }
}