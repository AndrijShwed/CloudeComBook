namespace ClaudeComBook.Desktop.Models;

public class DocumentTemplateDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public byte[]? Template { get; set; }
}
