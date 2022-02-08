namespace AtcWeb.Models;

public class DocsSectionLink
{
    public string Id { get; set; }

    public string Title { get; set; }

    public bool Active { get; set; }

    public override string ToString()
    {
        return $"{nameof(Id)}: {Id}, {nameof(Title)}: {Title}, {nameof(Active)}: {Active}";
    }
}