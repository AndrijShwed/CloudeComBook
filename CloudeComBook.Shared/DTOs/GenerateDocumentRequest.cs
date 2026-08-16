namespace CloudeComBook.Shared.DTOs;

public class GenerateDocumentRequest
{
    public string TemplateType { get; set; } = "";
    public int PersonId { get; set; }
    public Dictionary<string, string> ExtraFields { get; set; } = new();
}
