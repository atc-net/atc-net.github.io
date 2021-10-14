using System;

namespace AtcWeb.Models
{
    public class AtcComponent
    {
        public string Name { get; set; }
        
        public string Link { get; set; }
        
        public bool IsNavGroup { get; set; }
        
        public bool NavGroupExpanded { get; set; }
        
        public DocsComponents GroupItems { get; set; }
        
        public Type Component { get; set; }
        
        public Type[] ChildComponents { get; set; }

        public string ComponentName => Component.Name.Replace("`1", "<T>");

        public override string ToString()
        {
            return $"{nameof(Name)}: {Name}, {nameof(Link)}: {Link}, {nameof(IsNavGroup)}: {IsNavGroup}, {nameof(NavGroupExpanded)}: {NavGroupExpanded}, {nameof(GroupItems)}: {GroupItems}, {nameof(Component)}: {Component}, {nameof(ChildComponents)}: {ChildComponents}, {nameof(ComponentName)}: {ComponentName}";
        }
    }
}