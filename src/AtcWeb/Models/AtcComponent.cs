using System;

namespace AtcWeb.Models
{
    public class AtcComponent
    {
        public string Name { get; set; } = string.Empty;

        public string Link { get; set; } = string.Empty;

        public bool IsNavGroup { get; set; }

        public bool NavGroupExpanded { get; set; }

        public DocsComponents GroupItems { get; set; }

        public Type Component { get; set; }

        public Type[] ChildComponents { get; set; }

        public string ComponentName => Component.Name.Replace("`1", "<T>", StringComparison.Ordinal);

        public override string ToString()
            => $"{nameof(Name)}: {Name}, {nameof(Link)}: {Link}, {nameof(IsNavGroup)}: {IsNavGroup}, {nameof(NavGroupExpanded)}: {NavGroupExpanded}, {nameof(GroupItems)}: {GroupItems}, {nameof(Component)}: {Component}, {nameof(ChildComponents)}: {ChildComponents}, {nameof(ComponentName)}: {ComponentName}";
    }
}