namespace AtcWeb.Domain.GitHub.Models;

public class DocumentMetadata
{
    public string Title { get; set; }

    public string Body { get; set; }

    public List<DocumentSectionMetadata> SubSection { get; set; } = new();
}