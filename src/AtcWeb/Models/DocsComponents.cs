using System;
using System.Collections.Generic;
using System.Linq;
using Atc;

namespace AtcWeb.Models
{
    public class DocsComponents
    {
        private readonly List<AtcComponent> atcComponents = new ();

        /// <summary>
        /// The elements of the list of atc-components
        /// </summary>
        internal IEnumerable<AtcComponent> Elements => atcComponents.OrderBy(e => e.Name);

        public DocsComponents AddItem(string name, Type component, params Type[] childComponents)
        {
            var componentItem = new AtcComponent
            {
                Name = name,
                Link = name?.ToLower(GlobalizationConstants.EnglishCultureInfo).Replace(" ", string.Empty, StringComparison.Ordinal) ?? string.Empty,
                Component = component,
                ChildComponents = childComponents,
                IsNavGroup = false,
            };

            atcComponents.Add(componentItem);

            return this;
        }

        public DocsComponents AddNavGroup(string name, bool expanded, DocsComponents groupItems)
        {
            var componentItem = new AtcComponent
            {
                Name = name,
                NavGroupExpanded = expanded,
                GroupItems = groupItems,
                IsNavGroup = true,
            };

            atcComponents.Add(componentItem);

            return this;
        }
    }
}