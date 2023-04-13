namespace AtcWeb.Domain.GitHub.Models;

public class DocumentSectionMetadata
{
    public int Level { get; set; }

    public string Title { get; set; }

    public string Body { get; set; }

    public List<DocumentSectionMetadata> SubSection { get; set; } = new();
}