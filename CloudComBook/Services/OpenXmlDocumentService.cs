using CloudComBook.Shared.Models;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace CloudComBook.API.Services;

public class OpenXmlDocumentService
{
    private const string FamilyMarker = "{FR}";

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
            var main = doc.MainDocumentPart
                ?? throw new InvalidOperationException("Шаблон не містить тіла документа.");
            var body = main.Document.Body
                ?? throw new InvalidOperationException("Шаблон порожній.");

            var replacer = new PlaceholderReplacer(fields);

            replacer.Replace(body);
            foreach (var header in main.HeaderParts) replacer.Replace(header.Header);
            foreach (var footer in main.FooterParts) replacer.Replace(footer.Footer);

            //if (familyMembers != null)
            //    FillFamilyTable(body, familyMembers);

            //Обробка членів сім'ї
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

            main.Document.Save();
        }

        return stream.ToArray();
    }

    //============================================================
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

    // ---------- таблиця членів сім'ї ----------

    private static void FillFamilyTable(Body body, List<Person> familyMembers)
    {
        foreach (var table in body.Descendants<Table>().ToList())
        {
            var templateRow = table.Elements<TableRow>()
                .FirstOrDefault(r => string.Concat(r.Descendants<Text>().Select(t => t.Text))
                    .Contains(FamilyMarker));

            if (templateRow == null) continue;

            // рядок із маркером це рядок заявника (№ 1), тому члени сім'ї нумеруються з 2
            int number = 2;
            TableRow last = templateRow;

            foreach (var person in familyMembers)
            {
                var newRow = (TableRow)templateRow.CloneNode(true);
                FillFamilyRow(newRow, person, number++);
                last.InsertAfterSelf(newRow);
                last = newRow;
            }

            new PlaceholderReplacer(new Dictionary<string, string> { [FamilyMarker] = "" })
                .Replace(templateRow);
            return;
        }
    }

    private static void FillFamilyRow(TableRow row, Person person, int number)
    {
        var cells = row.Elements<TableCell>().ToList();

        SetCellText(cells, 0, number.ToString());
        SetCellText(cells, 1, $"{person.LastName} {person.Name} {person.Surname}".Trim());
        SetCellText(cells, 2, "член сім'ї");
        SetCellText(cells, 3, person.DateOfBirth?.ToString("dd.MM.yyyy") ?? "");
        SetCellText(cells, 4, FormatDocument(person.Passport));
    }

    private static void SetCellText(List<TableCell> cells, int index, string value)
    {
        if (index >= cells.Count) return;
        var cell = cells[index];

        var texts = cell.Descendants<Text>().ToList();
        if (texts.Count > 0)
        {
            texts[0].Text = value;
            texts[0].Space = SpaceProcessingModeValues.Preserve;
            foreach (var t in texts.Skip(1)) t.Text = "";
            return;
        }

        // порожня комірка: створюємо текст самі
        var paragraph = cell.Elements<Paragraph>().FirstOrDefault();
        if (paragraph == null)
        {
            paragraph = new Paragraph();
            cell.Append(paragraph);
        }
        paragraph.Append(new Run(new Text(value) { Space = SpaceProcessingModeValues.Preserve }));
    }

    public static string FormatDocument(string? document)
    {
        document ??= "";
        char[] result = Enumerable.Repeat(' ', 11).ToArray();

        for (int i = 0; i < Math.Min(4, document.Length); i++)
            result[i] = document[i];

        for (int i = 4; i < 11 && i < document.Length; i++)
            result[i] = char.IsDigit(document[i]) ? document[i] : ' ';

        return new string(result);
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

    // ---------- заміна плейсхолдерів зі збереженням форматування ----------

    private sealed class PlaceholderReplacer
    {
        private readonly List<KeyValuePair<string, string>> _fields;

        public PlaceholderReplacer(IDictionary<string, string> fields)
        {
            // довші ключі перевіряються першими: «Посада_1» раніше за «Посада»
            _fields = fields
                .Where(f => !string.IsNullOrEmpty(f.Key))
                .Select(f => new KeyValuePair<string, string>(Normalize(f.Key), f.Value ?? ""))
                .OrderByDescending(f => f.Key.Length)
                .ToList();
        }

        public void Replace(OpenXmlElement root)
        {
            foreach (var paragraph in root.Descendants<Paragraph>().ToList())
                ReplaceInParagraph(paragraph);
        }

        private void ReplaceInParagraph(Paragraph paragraph)
        {
            // лише текст самого абзацу (вкладені абзаци текстових полів обробляються окремо)
            var texts = paragraph.Descendants<Text>()
                .Where(t => ReferenceEquals(t.Ancestors<Paragraph>().FirstOrDefault(), paragraph))
                .ToList();
            if (texts.Count == 0) return;

            var lengths = texts.Select(t => (t.Text ?? "").Length).ToArray();
            var starts = new int[texts.Count];
            int pos = 0;
            for (int k = 0; k < texts.Count; k++)
            {
                starts[k] = pos;
                pos += lengths[k];
            }

            var normalized = Normalize(string.Concat(texts.Select(t => t.Text ?? "")));

            // один прохід зліва направо: замінений текст повторно не перевіряється
            var matches = new List<(int Start, int Length, string Value)>();
            int i = 0;
            while (i < normalized.Length)
            {
                bool found = false;
                foreach (var f in _fields)
                {
                    if (f.Key.Length <= normalized.Length - i &&
                        string.Compare(normalized, i, f.Key, 0, f.Key.Length,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        matches.Add((i, f.Key.Length, f.Value));
                        i += f.Key.Length;
                        found = true;
                        break;
                    }
                }
                if (!found) i++;
            }

            // застосовуємо з кінця, щоб позиції попередніх збігів не зсувалися
            for (int m = matches.Count - 1; m >= 0; m--)
            {
                var (start, length, value) = matches[m];
                int end = start + length;

                int first = FindText(starts, lengths, start);
                int last = FindText(starts, lengths, end - 1);
                if (first < 0 || last < 0) continue;

                int startLocal = start - starts[first];
                int endLocal = end - starts[last];

                var firstStr = texts[first].Text ?? "";

                if (first == last)
                {
                    texts[first].Text = firstStr.Substring(0, startLocal) + value + firstStr.Substring(endLocal);
                }
                else
                {
                    texts[first].Text = firstStr.Substring(0, startLocal) + value;
                    for (int k = first + 1; k < last; k++) texts[k].Text = "";
                    texts[last].Text = (texts[last].Text ?? "").Substring(endLocal);
                }

                for (int k = first; k <= last; k++)
                    texts[k].Space = SpaceProcessingModeValues.Preserve;
            }
        }

        private static int FindText(int[] starts, int[] lengths, int index)
        {
            for (int k = 0; k < starts.Length; k++)
                if (index >= starts[k] && index < starts[k] + lengths[k]) return k;
            return -1;
        }

        // довжина рядка не змінюється, тож позиції збігаються з оригіналом
        private static string Normalize(string s)
        {
            var chars = s.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                chars[i] = chars[i] switch
                {
                    '’' or 'ʼ' or '‘' or '`' => '\'',
                    '\u00A0' => ' ',
                    var c => c
                };
            }
            return new string(chars);
        }
    }
}
