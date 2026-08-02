using ClaudeComBook.Desktop.Models;
using DocumentFormat.OpenXml;
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
    public byte[] FillTemplate(
    byte[] templateBytes,
    Dictionary<string, string> fields,
    List<Person>? familyMembers = null)
    {
        using var memStream = new MemoryStream();
        memStream.Write(templateBytes, 0, templateBytes.Length);
        memStream.Position = 0;

        using (var doc = WordprocessingDocument.Open(memStream, true))
        {
            var body = doc.MainDocumentPart?.Document?.Body;
            if (body == null)
                return templateBytes;

            // Зберігаємо незмінений рядок-шаблон
            Table? familyTable = null;
            TableRow? serviceRow = null;
            TableRow? familyTemplate = null;

            if (familyMembers != null)
            {
                familyTable = body.Descendants<Table>().FirstOrDefault();

                if (familyTable != null)
                {
                    var rows = familyTable.Elements<TableRow>().ToList();

                    // 0 - заголовок
                    // 1 - заявник
                    // 2 - шаблон
                    if (rows.Count >= 3)
                    {
                        serviceRow = rows[2];
                        familyTemplate = (TableRow)serviceRow.CloneNode(true);
                    }
                        
                }
            }

            ReplaceMarkers(body, fields, serviceRow);


            // Заповнюємо таблицю
            if (familyTable != null &&
                 serviceRow != null &&
                 familyTemplate != null)
            {
                FillFamilyTable(
                    familyTable,
                    serviceRow,
                    familyTemplate,
                    familyMembers ?? new List<Person>());
            }

            doc.MainDocumentPart.Document.Save();
        }

        return memStream.ToArray();
    }

    private void FillFamilyTable(
                     Table table,
                     TableRow serviceRow,
                     TableRow templateRow,
                     List<Person> familyMembers)
    {
        if (table == null)
            return;

        // службовий рядок у документі
        templateRow = (TableRow)serviceRow.CloneNode(true);

        // Якщо немає членів сім'ї — просто прибираємо службовий рядок
        if (familyMembers.Count == 0)
        {
            serviceRow.Remove();
            return;
        }

        int index = 2;

        foreach (var person in familyMembers)
        {
            var row = (TableRow)templateRow.CloneNode(true);

            ReplaceCell(row, "{№}", index.ToString());

            ReplaceCell(row, "full_name",
                $"{person.LastName} {person.Name} {person.Surname}");

            ReplaceCell(row, "birth_date",
                person.DateOfBirth?.ToString("dd.MM.yyyy") ?? "");

            ReplaceCell(row, "Документ",
                        person.Passport is { Length: > 9 }
                            ? person.Passport[..9]
                            : person.Passport ?? "");

            ReplaceCell(row, "член сім'ї", "член сім'ї");

            table.InsertBefore(row, serviceRow);

            index++;
        }

        // Видаляємо службовий рядок
        serviceRow.Remove();
    }

    private void ReplaceCell(TableRow row, string key, string value)
    {
        var fields = new Dictionary<string, string>
    {
        { key, value }
    };

        foreach (var paragraph in row.Descendants<Paragraph>())
        {
            ReplaceParagraphText(paragraph, fields);
        }
    }

    private void ReplaceParagraphText(Paragraph paragraph, Dictionary<string, string> fields)
    {
        var runs = paragraph.Descendants<Run>().ToList();

        if (runs.Count == 0)
            return;

        string fullText = string.Concat(
            runs.SelectMany(r => r.Elements<Text>())
                .Select(t => t.Text));

        if (string.IsNullOrEmpty(fullText))
            return;

        bool changed = false;

        foreach (var field in fields)
        {
            if (fullText.Contains(field.Key))
            {
                fullText = fullText.Replace(field.Key, field.Value ?? "");
                changed = true;
            }
        }

        if (!changed)
            return;

        var firstRun = runs.First();

        var runProperties = firstRun.RunProperties?.CloneNode(true);

        foreach (var run in runs)
            run.Remove();

        var newRun = new Run();

        if (runProperties != null)
            newRun.RunProperties = (RunProperties)runProperties;

        var lines = fullText.Replace("\r\n", "\n").Split('\n');

        for (int i = 0; i < lines.Length; i++)
        {
            newRun.AppendChild(new Text(lines[i])
            {
                Space = SpaceProcessingModeValues.Preserve
            });

            if (i < lines.Length - 1)
                newRun.AppendChild(new Break());
        }

        paragraph.AppendChild(newRun);
    }

    //private void ReplaceSplitRuns(Paragraph paragraph, Dictionary<string, string> fields)
    //{
    //    var texts = paragraph.Descendants<Text>().ToList();

    //    if (texts.Count != 2)
    //        return;

    //    string key = texts[0].Text + texts[1].Text;

    //    System.Diagnostics.Debug.WriteLine($"KEY = [{key}]");

    //    if (fields.TryGetValue(key, out var value))
    //    {
    //        System.Diagnostics.Debug.WriteLine($"FOUND = {key} -> {value}");

    //        texts[0].Text = value;
    //        texts[1].Text = "";
    //    }
    //}

    private void ReplaceMarkers(
     Body body,
     Dictionary<string, string> fields,
     TableRow? serviceRow)
    {
        foreach (var paragraph in body.Descendants<Paragraph>())
        {
            // Не чіпаємо службовий рядок таблиці
            if (serviceRow != null &&
                paragraph.Ancestors<TableRow>().FirstOrDefault() == serviceRow)
                continue;

            foreach (var text in paragraph.Descendants<Text>())
            {
                if (fields.TryGetValue(text.Text, out var value))
                {
                    text.Text = value;
                }
            }
        }
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
