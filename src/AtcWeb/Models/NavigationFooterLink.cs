namespace AtcWeb.Models
{
    public class NavigationFooterLink
    {
        public NavigationFooterLink()
        {
        }

        public NavigationFooterLink(string name, string link)
        {
            Name = name;
            Link = link;
        }

        public string Name { get; set; }

        public string Link { get; set; }

        public override string ToString()
        {
            return $"{nameof(Name)}: {Name}, {nameof(Link)}: {Link}";
        }
    }
}