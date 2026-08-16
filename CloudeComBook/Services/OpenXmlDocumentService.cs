//using CloudeComBook.Shared.Models;
//using DocumentFormat.OpenXml.Packaging;
//using DocumentFormat.OpenXml.Wordprocessing;

//namespace CloudeComBook.API.Services;

//public class OpenXmlDocumentService
//{
//    public byte[] GenerateDocument(
//        byte[] templateBytes,
//        Dictionary<string, string> fields,
//        List<Person>? familyMembers = null)
//    {
//        using var stream = new MemoryStream();
//        stream.Write(templateBytes, 0, templateBytes.Length);
//        stream.Position = 0;

//        using (var doc = WordprocessingDocument.Open(stream, true))
//        {
//            var body = doc.MainDocumentPart!.Document.Body!;

//            MergeRunsInDocument(doc);
//            ReplaceTextPlaceholders(body, fields);

//            if (familyMembers != null)
//                FillFamilyTable(body, familyMembers);

//            doc.MainDocumentPart.Document.Save();
//        }

//        return stream.ToArray();
//    }

//    // Word часто розбиває один текст на кілька <w:r> - об'єднуємо їх у межах кожного параграфа
//    private void MergeRunsInDocument(WordprocessingDocument doc)
//    {
//        var paragraphs = doc.MainDocumentPart!.Document.Body!.Descendants<Paragraph>().ToList();

//        foreach (var paragraph in paragraphs)
//        {
//            var runs = paragraph.Elements<Run>().ToList();
//            if (runs.Count <= 1) continue;

//            var firstRun = runs[0];
//            var texts = firstRun.Elements<Text>().ToList();
//            if (texts.Count == 0) continue;

//            var combinedText = string.Concat(runs.SelectMany(r =>
//                r.Elements<Text>().Select(t => t.Text)));

//            // Залишаємо лише перший Text у першому Run, решту прибираємо
//            foreach (var t in texts.Skip(1))
//                t.Remove();

//            texts[0].Text = combinedText;
//            texts[0].Space = DocumentFormat.OpenXml.SpaceProcessingModeValues.Preserve;

//            foreach (var run in runs.Skip(1))
//                run.Remove();
//        }
//    }

//    private void ReplaceTextPlaceholders(Body body, Dictionary<string, string> fields)
//    {
//        foreach (var text in body.Descendants<Text>())
//        {
//            foreach (var field in fields)
//            {
//                if (text.Text.Contains(field.Key))
//                    text.Text = text.Text.Replace(field.Key, field.Value ?? "");
//            }
//        }
//    }

//    private void FillFamilyTable(Body body, List<Person> familyMembers)
//    {
//        foreach (var table in body.Descendants<Table>())
//        {
//            TableRow? templateRow = null;

//            foreach (var row in table.Elements<TableRow>())
//            {
//                if (RowContainsMarker(row, "{FR}"))
//                {
//                    templateRow = row;
//                    break;
//                }
//            }

//            if (templateRow == null) continue;

//            int number = 1;
//            TableRow lastInsertedRow = templateRow;

//            foreach (var person in familyMembers)
//            {
//                var newRow = (TableRow)templateRow.CloneNode(true);
//                FillFamilyRow(newRow, person, number);
//                lastInsertedRow.InsertAfterSelf(newRow);
//                lastInsertedRow = newRow;
//                number++;
//            }

//            // Прибираємо шаблонний рядок з маркером {FR}
//            ClearMarkerInRow(templateRow, "{FR}");
//        }
//    }

//    private void FillFamilyRow(TableRow row, Person person, int number)
//    {
//        var cells = row.Elements<TableCell>().ToList();

//        SetCellText(cells, 0, number.ToString());
//        SetCellText(cells, 1, $"{person.LastName} {person.Name} {person.Surname}");
//        SetCellText(cells, 2, "член сім'ї");
//        SetCellText(cells, 3, person.DateOfBirth?.ToString("dd.MM.yyyy") ?? "");
//        SetCellText(cells, 4, FormatDocument(person.Passport));

//        ClearMarkerInRow(row, "{FR}");
//    }

//    private void SetCellText(List<TableCell> cells, int index, string value)
//    {
//        if (index >= cells.Count) return;

//        var text = cells[index].Descendants<Text>().FirstOrDefault();
//        if (text != null)
//            text.Text = value;
//    }

//    private void ClearMarkerInRow(TableRow row, string marker)
//    {
//        foreach (var text in row.Descendants<Text>())
//        {
//            if (text.Text.Contains(marker))
//                text.Text = text.Text.Replace(marker, "");
//        }
//    }

//    private bool RowContainsMarker(TableRow row, string marker)
//    {
//        var rowText = string.Concat(row.Descendants<Text>().Select(t => t.Text));
//        return rowText.Contains(marker);
//    }

//    public static string FormatDocument(string? document)
//    {
//        document ??= "";
//        char[] result = Enumerable.Repeat(' ', 11).ToArray();

//        for (int i = 0; i < Math.Min(4, document.Length); i++)
//            result[i] = document[i];

//        for (int i = 4; i < 11 && i < document.Length; i++)
//            result[i] = char.IsDigit(document[i]) ? document[i] : ' ';

//        return new string(result);
//    }
//}



using CloudeComBook.Shared.Models;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace CloudeComBook.API.Services;

public class OpenXmlDocumentService
{
    public byte[] GenerateDocument(
        byte[] templateBytes,
        Dictionary<string, string> fields,
        List<Person>? familyMembers = null)
    {
        using var stream = new MemoryStream();
        stream.Write(templateBytes, 0, templateBytes.Length);
        stream.Position = 0;

        using (var doc = WordprocessingDocument.Open(stream, true))
        {
            var body = doc.MainDocumentPart!.Document.Body!;

            // Об'єднуємо розбиті Word Run
            MergeRunsInDocument(doc);

            // Заміна звичайних текстових полів
            ReplaceTextPlaceholders(body, fields);

            // Обробка членів сім'ї
            if (familyMembers != null && familyMembers.Count > 0)
            {
                // Якщо в шаблоні є закладка FamilyMembers -
                // вставляємо нумерований список.
                if (HasBookmark(body, "FamilyMembers"))
                {
                    FillFamilyMembersBookmark(body, familyMembers);
                }

                // Якщо в шаблоні є таблиця з маркером {FR} -
                // заповнюємо її старим способом.
                FillFamilyTable(body, familyMembers);
            }

            doc.MainDocumentPart.Document.Save();
        }

        return stream.ToArray();
    }


    // ============================================================
    // ОБ'ЄДНАННЯ RUN
    // ============================================================

    // Word часто розбиває один текст на кілька <w:r>.
    // Об'єднуємо їх у межах кожного параграфа.
    private void MergeRunsInDocument(WordprocessingDocument doc)
    {
        var paragraphs = doc.MainDocumentPart!
            .Document
            .Body!
            .Descendants<Paragraph>()
            .ToList();

        foreach (var paragraph in paragraphs)
        {
            var runs = paragraph.Elements<Run>().ToList();

            if (runs.Count <= 1)
                continue;

            var firstRun = runs[0];

            var texts = firstRun
                .Elements<Text>()
                .ToList();

            if (texts.Count == 0)
                continue;

            var combinedText = string.Concat(
                runs.SelectMany(r =>
                    r.Elements<Text>()
                     .Select(t => t.Text)));

            // Залишаємо лише перший Text
            foreach (var t in texts.Skip(1))
                t.Remove();

            texts[0].Text = combinedText;
            texts[0].Space =
                DocumentFormat.OpenXml.SpaceProcessingModeValues.Preserve;

            // Видаляємо всі наступні Run
            foreach (var run in runs.Skip(1))
                run.Remove();
        }
    }


    // ============================================================
    // ЗАМІНА ЗВИЧАЙНИХ ТЕКСТОВИХ ПОЛІВ
    // ============================================================

    private void ReplaceTextPlaceholders(
        Body body,
        Dictionary<string, string> fields)
    {
        foreach (var text in body.Descendants<Text>())
        {
            foreach (var field in fields)
            {
                if (text.Text.Contains(field.Key))
                {
                    text.Text = text.Text.Replace(
                        field.Key,
                        field.Value ?? "");
                }
            }
        }
    }


    // ============================================================
    // ПЕРЕВІРКА НАЯВНОСТІ ЗАКЛАДКИ
    // ============================================================

    private bool HasBookmark(
        Body body,
        string bookmarkName)
    {
        return body
            .Descendants<BookmarkStart>()
            .Any(x => x.Name?.Value == bookmarkName);
    }


    // ============================================================
    // FAMILY MEMBERS — ЗАКЛАДКА FamilyMembers
    // ============================================================

    private void FillFamilyMembersBookmark(
        Body body,
        List<Person> familyMembers)
    {
        var bookmarkStart = body
            .Descendants<BookmarkStart>()
            .FirstOrDefault(x =>
                x.Name?.Value == "FamilyMembers");

        if (bookmarkStart == null)
            return;

        var bookmarkId = bookmarkStart.Id?.Value;

        if (bookmarkId == null)
            return;

        var bookmarkEnd = body
            .Descendants<BookmarkEnd>()
            .FirstOrDefault(x =>
                x.Id?.Value == bookmarkId);

        if (bookmarkEnd == null)
            return;

        // Параграф, у якому знаходиться закладка
        var bookmarkParagraph = bookmarkStart
            .Ancestors<Paragraph>()
            .FirstOrDefault();

        if (bookmarkParagraph == null)
            return;

        // Видаляємо існуючий текст між BookmarkStart і BookmarkEnd.
        RemoveBookmarkContent(bookmarkStart, bookmarkEnd);

        int number = 1;

        foreach (var person in familyMembers)
        {
            string fullName =
                $"{person.LastName} {person.Name} {person.Surname}"
                .Trim();

            string birthDate = person.DateOfBirth.HasValue
                ? person.DateOfBirth.Value.ToString("dd.MM.yyyy")
                : "";

            string line = $"{number}. {fullName}";

            if (!string.IsNullOrWhiteSpace(birthDate))
            {
                line += $", {birthDate} р.н.";
            }

            // Створюємо новий абзац.
            var newParagraph = new Paragraph();

            // Копіюємо стиль першого абзацу шаблону,
            // щоб список максимально відповідав оформленню документа.
            var paragraphProperties =
                    bookmarkParagraph.ParagraphProperties != null
                    ? (ParagraphProperties)bookmarkParagraph.ParagraphProperties.CloneNode(true)
                    : new ParagraphProperties();

            // Міжрядковий інтервал 1,5
            paragraphProperties.SpacingBetweenLines = new SpacingBetweenLines
            {
                Line = "360",
                LineRule = LineSpacingRuleValues.Auto
            };

            newParagraph.ParagraphProperties = paragraphProperties;

            var run = new Run();

            var runProperties = new RunProperties(
                new RunFonts
                {
                    Ascii = "Times New Roman",
                    HighAnsi = "Times New Roman",
                    EastAsia = "Times New Roman",
                    ComplexScript = "Times New Roman"
                },
                new FontSize
                {
                    Val = "28"
                },
                new FontSizeComplexScript
                {
                    Val = "28"
                }
            );

            run.RunProperties = runProperties;

            var text = new Text(line)
            {
                Space =
                    DocumentFormat.OpenXml
                        .SpaceProcessingModeValues.Preserve
            };

            run.AppendChild(text);
            newParagraph.AppendChild(run);

            // Вставляємо новий абзац після абзацу закладки.
            bookmarkParagraph.InsertAfterSelf(newParagraph);

            bookmarkParagraph = newParagraph;

            number++;
        }

        // Після вставки списку прибираємо порожній абзац,
        // який містив закладку.
        RemoveBookmarkIfEmpty(bookmarkStart, bookmarkEnd);
    }


    // ============================================================
    // ВИДАЛЕННЯ ВМІСТУ ЗАКЛАДКИ
    // ============================================================

    private void RemoveBookmarkContent(
        BookmarkStart bookmarkStart,
        BookmarkEnd bookmarkEnd)
    {
        var current = bookmarkStart.NextSibling();

        while (current != null &&
               current != bookmarkEnd)
        {
            var next = current.NextSibling();

            current.Remove();

            current = next;
        }
    }


    // ============================================================
    // ПРИБИРАННЯ ПОРОЖНЬОЇ ЗАКЛАДКИ
    // ============================================================

    private void RemoveBookmarkIfEmpty(
        BookmarkStart bookmarkStart,
        BookmarkEnd bookmarkEnd)
    {
        // Закладку залишаємо в документі.
        // Word може використовувати її надалі.
        //
        // Сам текст усередині вже очищений
        // методом RemoveBookmarkContent().
    }


    // ============================================================
    // FAMILY MEMBERS — ТАБЛИЦЯ {FR}
    // ============================================================

    private void FillFamilyTable(
        Body body,
        List<Person> familyMembers)
    {
        foreach (var table in body.Descendants<Table>())
        {
            TableRow? templateRow = null;

            foreach (var row in table.Elements<TableRow>())
            {
                if (RowContainsMarker(row, "{FR}"))
                {
                    templateRow = row;
                    break;
                }
            }

            // У цій таблиці немає {FR}
            if (templateRow == null)
                continue;

            int number = 1;

            TableRow lastInsertedRow = templateRow;

            foreach (var person in familyMembers)
            {
                var newRow =
                    (TableRow)templateRow.CloneNode(true);

                FillFamilyRow(
                    newRow,
                    person,
                    number);

                lastInsertedRow.InsertAfterSelf(newRow);

                lastInsertedRow = newRow;

                number++;
            }

            // Прибираємо шаблонний рядок
            // з маркером {FR}.
            ClearMarkerInRow(
                templateRow,
                "{FR}");
        }
    }


    // ============================================================
    // ЗАПОВНЕННЯ РЯДКА ТАБЛИЦІ
    // ============================================================

    private void FillFamilyRow(
        TableRow row,
        Person person,
        int number)
    {
        var cells = row
            .Elements<TableCell>()
            .ToList();

        SetCellText(
            cells,
            0,
            number.ToString());

        SetCellText(
            cells,
            1,
            $"{person.LastName} {person.Name} {person.Surname}");

        SetCellText(
            cells,
            2,
            "член сім'ї");

        SetCellText(
            cells,
            3,
            person.DateOfBirth?.ToString("dd.MM.yyyy") ?? "");

        SetCellText(
            cells,
            4,
            FormatDocument(person.Passport));

        ClearMarkerInRow(
            row,
            "{FR}");
    }


    // ============================================================
    // ЗАПИС ТЕКСТУ В КОМІРКУ
    // ============================================================

    private void SetCellText(
        List<TableCell> cells,
        int index,
        string value)
    {
        if (index >= cells.Count)
            return;

        var text = cells[index]
            .Descendants<Text>()
            .FirstOrDefault();

        if (text != null)
        {
            text.Text = value;
        }
    }


    // ============================================================
    // ВИДАЛЕННЯ МАРКЕРА З РЯДКА
    // ============================================================

    private void ClearMarkerInRow(
        TableRow row,
        string marker)
    {
        foreach (var text in row.Descendants<Text>())
        {
            if (text.Text.Contains(marker))
            {
                text.Text = text.Text.Replace(
                    marker,
                    "");
            }
        }
    }


    // ============================================================
    // ПЕРЕВІРКА РЯДКА НА МАРКЕР
    // ============================================================

    private bool RowContainsMarker(
        TableRow row,
        string marker)
    {
        var rowText = string.Concat(
            row.Descendants<Text>()
               .Select(t => t.Text));

        return rowText.Contains(marker);
    }


    // ============================================================
    // ФОРМАТУВАННЯ ПАСПОРТА
    // ============================================================

    public static string FormatDocument(
        string? document)
    {
        document ??= "";

        char[] result =
            Enumerable.Repeat(' ', 11).ToArray();

        for (int i = 0;
             i < Math.Min(4, document.Length);
             i++)
        {
            result[i] = document[i];
        }

        for (int i = 4;
             i < 11 && i < document.Length;
             i++)
        {
            result[i] =
                char.IsDigit(document[i])
                    ? document[i]
                    : ' ';
        }

        return new string(result);
    }
}
