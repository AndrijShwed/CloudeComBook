using ClaudeComBook.Shared.Models;
using Microsoft.Office.Interop.Word;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Word = Microsoft.Office.Interop.Word;

namespace ClaudeComBook.Desktop.Services;

public class DocumentService
{
    private Word.Application? _word;
    private Word.Document? _document;

    public string GenerateDocument(
        string templatePath,
        string outputFolder,
        string outputFileName,
        Dictionary<string, string> fields,
        List<Person>? familyMembers = null,
        bool openAfterSave = true)
    {
        Directory.CreateDirectory(outputFolder);

        string outputPath =
            Path.Combine(outputFolder, outputFileName);

        File.Copy(templatePath, outputPath, true);

        try
        {
            _word = new Application
            {
                Visible = false,
                DisplayAlerts = WdAlertLevel.wdAlertsNone
            };

            _document = _word.Documents.Open(outputPath);

             ReplaceWords(fields);

            // Якщо є закладка - заповнюємо її
            if (familyMembers != null && HasFamilyTable())
            {
                FillFamilyTable(familyMembers);
            }
            else if (familyMembers != null &&
                     _document.Bookmarks.Exists("FamilyMembers"))
            {
                FillFamilyBookmark(familyMembers);
            }


            _document.Save();

            _document.Close(false);

            _word.Quit(false);

            return outputPath;
        }
        finally
        {
            ReleaseObjects();

            if (openAfterSave)
            {
                OpenDocument(outputPath);
            }
        }
    }

    private bool HasFamilyTable()
    {
        if (_document == null)
            return false;

        foreach (Word.Table table in _document.Tables)
        {
            foreach (Word.Row row in table.Rows)
            {
                if (RowContainsMarker(row, "{FR}"))
                    return true;
            }
        }

        return false;
    }

    private void ReplaceWords(Dictionary<string, string> fields)
    {
        if (_document == null)
            return;

        foreach (var field in fields)
        {
            Word.Range range = _document.Content;

            Word.Find find = range.Find;

            find.ClearFormatting();
            find.Replacement.ClearFormatting();

            find.Text = field.Key;
            find.Replacement.Text = field.Value ?? "";

            find.Forward = true;
            find.Wrap = Word.WdFindWrap.wdFindContinue;
            find.Format = false;
            find.MatchCase = false;
            find.MatchWholeWord = false;
            find.MatchWildcards = false;

            find.Execute(
                Replace: Word.WdReplace.wdReplaceAll);
        }
    }

    public void OpenDocument(string filePath)
    {
        System.Diagnostics.Process.Start(
            new System.Diagnostics.ProcessStartInfo
            {
                FileName = filePath,
                UseShellExecute = true
            });
    }

    private void ReleaseObjects()
    {
        if (_document != null)
        {
            Marshal.ReleaseComObject(_document);
            _document = null;
        }

        if (_word != null)
        {
            Marshal.ReleaseComObject(_word);
            _word = null;
        }

        GC.Collect();
        GC.WaitForPendingFinalizers();

        GC.Collect();
        GC.WaitForPendingFinalizers();
    }

    private void FillFamilyTable(List<Person> familyMembers)
    {
        if (_document == null)
            return;

        Word.Table? table = null;
        Word.Row? templateRow = null;

        foreach (Word.Table t in _document.Tables)
        {
            foreach (Word.Row row in t.Rows)
            {
                if (RowContainsMarker(row, "{FR}"))
                {
                    table = t;
                    templateRow = row;
                    break;
                }
            }

            if (table != null)
                break;
        }

        if (table == null || templateRow == null)
            return;

        int number = 2;

        foreach (var person in familyMembers)
        {
            templateRow = table.Rows[2];

            Word.Row newRow = table.Rows.Add();

            FillFamilyRow(newRow, person, number);

            number++;
        }

    }

    private void FillFamilyBookmark(List<Person> familyMembers)
    {
        if (_document == null)
            return;

        if (!_document.Bookmarks.Exists("FamilyMembers"))
            return;

        Word.Range range = _document.Bookmarks["FamilyMembers"].Range;

        if (familyMembers == null || familyMembers.Count == 0)
        {
            range.Text = "за даною адресою особа проживає одна";
        }
        else
        {
            range.Text = string.Join(
                Environment.NewLine + Environment.NewLine,
                familyMembers.Select((p, i) =>
                    $"{i + 1}. {p.LastName} {p.Name} {p.Surname} - {p.DateOfBirth:dd.MM.yyyy} р.н."));
        }

        // Повертаємо закладку, оскільки після присвоєння Text вона видаляється
        _document.Bookmarks.Add("FamilyMembers", range);
    }

    private void FillFamilyRow(
     Word.Row row,
     Person person,
     int number)
    {
        SetCell(row, 1, number.ToString());

        SetCell(row, 2,
            $"{person.LastName} {person.Name} {person.Surname}");

        SetCell(row, 3, "член сім'ї");

        SetCell(row, 4,
            person.DateOfBirth?.ToString("dd.MM.yyyy") ?? "");

        SetCell(row, 5, FormatDocument(person.Passport));

        ReplaceInRange(row.Range, "{FR}", "");
    }

    public static string FormatDocument(string? document)
    {
        document ??= "";

        char[] result = Enumerable.Repeat(' ', 11).ToArray();

        // Перші 4 символи копіюємо як є
        for (int i = 0; i < Math.Min(4, document.Length); i++)
        {
            result[i] = document[i];
        }

        // З 5-го по 11-й символ тільки цифри
        for (int i = 4; i < 11 && i < document.Length; i++)
        {
            result[i] = char.IsDigit(document[i])
                ? document[i]
                : ' ';
        }

        return new string(result);
    }

    private void SetCell(Word.Row row, int cellIndex, string value)
    {
        if (cellIndex > row.Cells.Count)
            return;

        Word.Range range = row.Cells[cellIndex].Range;

        // Видаляємо службові символи Word
        range.End -= 1;
        range.Text = value;
    }

    private void ReplaceInRange(
    Word.Range range,
    string findText,
    string replaceText)
    {
        Word.Find find = range.Find;

        find.ClearFormatting();
        find.Replacement.ClearFormatting();

        object replace = WdReplace.wdReplaceAll;
        object missing = Type.Missing;

        find.Execute(
            FindText: findText,
            MatchCase: false,
            MatchWholeWord: false,
            MatchWildcards: false,
            MatchSoundsLike: missing,
            MatchAllWordForms: false,
            Forward: true,
            Wrap: WdFindWrap.wdFindContinue,
            Format: false,
            ReplaceWith: replaceText,
            Replace: WdReplace.wdReplaceAll);
    }

    private bool RowContainsMarker(Word.Row row, string marker)
    {
        for (int i = 1; i <= row.Cells.Count; i++)
        {
            Word.Cell cell = row.Cells[i];

            string text = cell.Range.Text
                .Replace("\r", "")
                .Replace("\a", "")
                .Trim();

            if (text.Contains(marker))
                return true;
        }

        return false;
    }
}
