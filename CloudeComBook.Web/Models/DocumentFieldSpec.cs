namespace CloudeComBook.Web.Models;

public class DocumentFieldSpec
{
    public string Key { get; set; } = "";
    public string Label { get; set; } = "";
    public bool Required { get; set; } = true;
    public bool IsDate { get; set; }
    public bool FullWidth { get; set; }
}
