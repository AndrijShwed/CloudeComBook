namespace CloudComBook.Shared.DTOs;

public class DocumentTemplateStatusDto
{
    public string Type { get; set; } = "";
    public bool Exists { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
