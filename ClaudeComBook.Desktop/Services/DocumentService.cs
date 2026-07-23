using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ClaudeComBook.Desktop.Services;

public class DocumentService
{
    public byte[] FillTemplate(byte[] templateBytes, Dictionary<string, string> fields)
    {
        using var memStream = new MemoryStream();
        memStream.Write(templateBytes, 0, templateBytes.Length);
        memStream.Position = 0;

        using (var doc = WordprocessingDocument.Open(memStream, true))
        {
            var body = doc.MainDocumentPart?.Document?.Body;
            if (body == null) return templateBytes;

            foreach (var paragraph in body.Descendants<Paragraph>())
            {
                ReplaceParagraphText(paragraph, fields);
            }

            doc.MainDocumentPart!.Document.Save();
        }

        return memStream.ToArray();
    }

    private void ReplaceParagraphText(Paragraph paragraph, Dictionary<string, string> fields)
    {
        // Збираємо весь текст параграфа
        var runs = paragraph.Descendants<Run>().ToList();
        var fullText = string.Concat(runs.Select(r => r.InnerText));

        // Перевіряємо чи є що замінювати
        bool hasReplacement = fields.Keys.Any(key => fullText.Contains(key));
        if (!hasReplacement) return;

        // Замінюємо
        foreach (var field in fields)
            fullText = fullText.Replace(field.Key, field.Value);

        // Очищаємо старі runs і вставляємо новий
        var firstRun = runs.FirstOrDefault();
        if (firstRun == null) return;

        // Зберігаємо форматування першого run
        var runProperties = firstRun.RunProperties?.CloneNode(true);

        // Видаляємо всі runs
        foreach (var run in runs)
            run.Remove();

        // Додаємо новий run з заміненим текстом
        var newRun = new Run();
        if (runProperties != null)
            newRun.AppendChild(runProperties);
        newRun.AppendChild(new Text(fullText) { Space = DocumentFormat.OpenXml.SpaceProcessingModeValues.Preserve });
        paragraph.AppendChild(newRun);
    }

    public string SaveDocument(byte[] documentBytes, string folderPath, string fileName)
    {
        Directory.CreateDirectory(folderPath);
        var filePath = Path.Combine(folderPath, fileName);
        File.WriteAllBytes(filePath, documentBytes);
        return filePath;
    }

    public void OpenDocument(string filePath)
    {
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = filePath,
            UseShellExecute = true
        });
    }
}
