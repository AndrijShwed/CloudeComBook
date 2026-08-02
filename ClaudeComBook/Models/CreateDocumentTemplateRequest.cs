namespace ClaudeComBook.API.Models;

public class CreateDocumentTemplateRequest
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public IFormFile File { get; set; } = default!;
}
