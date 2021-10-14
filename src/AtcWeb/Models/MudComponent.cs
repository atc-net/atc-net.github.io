using System;

namespace AtcWeb.Models
{
    public class MudComponent
    {
        public string Name { get; set; }
        
        public string Link { get; set; }
        
        public bool IsNavGroup { get; set; }
        
        public bool NavGroupExpanded { get; set; }
        
        public DocsComponents GroupItems { get; set; }
        
        public Type Component { get; set; }
        
        public Type[] ChildComponents { get; set; }

        public string ComponentName => Component.Name.Replace("`1", "<T>");
    }
}