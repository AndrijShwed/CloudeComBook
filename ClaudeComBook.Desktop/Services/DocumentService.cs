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

            // Замінюємо всі текстові поля
            //foreach (var paragraph in body.Descendants<Paragraph>())
            //{
            //    // Пропускаємо параграфи службового рядка
            //    if (serviceRow != null &&
            //        paragraph.Ancestors<TableRow>().FirstOrDefault() == serviceRow)
            //    {
            //        continue;
            //    }

            //    ReplaceParagraphText(paragraph, fields);
            //}

            // Спочатку швидка заміна простих маркерів
            ReplaceSingleTexts(body, fields);

            // Потім тільки якщо Word розбив маркер на декілька Run
            foreach (var paragraph in body.Descendants<Paragraph>())
            {
                if (serviceRow != null &&
                    paragraph.Ancestors<TableRow>().FirstOrDefault() == serviceRow)
                    continue;

                ReplaceBrokenMarkers(paragraph, fields);
            }


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

    //private void ReplaceCell(TableRow row, string key, string value)
    //{
    //    foreach (var paragraph in row.Descendants<Paragraph>())
    //    {
    //        ReplaceParagraphText(
    //            paragraph,
    //            new Dictionary<string, string>
    //            {
    //            { key, value }
    //            });
    //    }
    //}

    private void ReplaceCell(TableRow row, string key, string value)
    {
        var dict = new Dictionary<string, string>
    {
        { key, value }
    };

        foreach (var text in row.Descendants<Text>())
        {
            if (dict.TryGetValue(text.Text.Trim(), out var v))
                text.Text = v;
        }

        foreach (var paragraph in row.Descendants<Paragraph>())
        {
            ReplaceBrokenMarkers(paragraph, dict);
        }
    }

    //private void ReplaceParagraphText(Paragraph paragraph, Dictionary<string, string> fields)
    //{
    //    // Збираємо весь текст параграфа
    //    var runs = paragraph.Descendants<Run>().ToList();
    //    var fullText = string.Concat(runs.Select(r => r.InnerText));

    //    // Перевіряємо чи є що замінювати
    //    bool hasReplacement = fields.Keys.Any(key => fullText.Contains(key));
    //    if (!hasReplacement) return;

    //    // Замінюємо
    //    foreach (var field in fields)
    //        fullText = fullText.Replace(field.Key, field.Value);

    //    // Очищаємо старі runs і вставляємо новий
    //    var firstRun = runs.FirstOrDefault();
    //    if (firstRun == null) return;

    //    // Зберігаємо форматування першого run
    //    var runProperties = firstRun.RunProperties?.CloneNode(true);

    //    // Видаляємо всі runs
    //    foreach (var run in runs)
    //        run.Remove();

    //    // Додаємо новий run із підтримкою перенесення рядків
    //    var newRun = new Run();

    //    if (runProperties != null)
    //        newRun.AppendChild(runProperties);

    //    var lines = fullText.Replace("\r\n", "\n").Split('\n');

    //    for (int i = 0; i < lines.Length; i++)
    //    {
    //        newRun.AppendChild(new Text(lines[i])
    //        {
    //            Space = DocumentFormat.OpenXml.SpaceProcessingModeValues.Preserve
    //        });

    //        if (i < lines.Length - 1)
    //        {
    //            newRun.AppendChild(new Break());
    //        }
    //    }

    //    paragraph.AppendChild(newRun);
    //}

    private void ReplaceSingleTexts(
    Body body,
    Dictionary<string, string> fields)
    {
        foreach (var text in body.Descendants<Text>())
        {
            var key = text.Text.Trim();

            if (fields.TryGetValue(key, out var value))
            {
                text.Text = value;
            }
        }
    }

    private void ReplaceBrokenMarkers(
    Paragraph paragraph,
    Dictionary<string, string> fields)
    {
        var texts = paragraph.Descendants<Text>().ToList();

        if (texts.Count < 2)
            return;

        string fullText = string.Concat(texts.Select(t => t.Text));

        bool changed = false;

        foreach (var field in fields)
        {
            if (fullText.Contains(field.Key))
            {
                fullText = fullText.Replace(field.Key, field.Value);
                changed = true;
            }
        }

        if (!changed)
            return;

        // Очищаємо тільки текст, НЕ видаляємо Run
        foreach (var text in texts)
            text.Text = "";

        var lines = fullText.Replace("\r\n", "\n").Split('\n');

        texts[0].Text = lines[0];

        if (lines.Length > 1)
        {
            var run = texts[0].Parent as Run;

            for (int i = 1; i < lines.Length; i++)
            {
                run?.AppendChild(new Break());

                run?.AppendChild(new Text(lines[i])
                {
                    Space = SpaceProcessingModeValues.Preserve
                });
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
