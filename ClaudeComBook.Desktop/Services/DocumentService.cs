using ClaudeComBook.Desktop.Models;
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

            ReplaceFields(fields);

            if (familyMembers != null)
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
    private void ReplaceFields(Dictionary<string, string> fields)
    {
        if (_document == null)
            return;

        foreach (var field in fields)
        {
            ReplaceInRange(
                _document.Content,
                "{" + field.Key + "}",
                field.Value ?? "");
        }
    }

    private void FillFamilyTable(List<Person> familyMembers)
    {
        if (_document == null)
            return;

        Row? templateRow = null;
        Table? familyTable = null;

        foreach (Table table in _document.Tables)
        {
            foreach (Row row in table.Rows)
            {
                if (RowContainsMarker(row, "{FamilyRow}"))
                {
                    templateRow = row;
                    familyTable = table;
                    break;
                }
            }

            if (templateRow != null)
                break;
        }

        if (templateRow == null || familyTable == null)
            return;

        if (familyMembers.Count == 0)
        {
            templateRow.Delete();
            return;
        }

        int number = 2;

        foreach (var person in familyMembers)
        {
            Word.Row newRow = familyTable.Rows.Add(templateRow);
            newRow.Range.FormattedText = templateRow.Range.FormattedText;

            ReplaceInRange(newRow.Range, "{FamilyRow}", "");
            FillFamilyRow(newRow, person, number);

            number++;
        }

        templateRow.Delete();
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
        ReplaceInRange(row.Range, "{№}", number.ToString());

        ReplaceInRange(row.Range,
            "{full_name}",
            $"{person.LastName} {person.Name} {person.Surname}");

        ReplaceInRange(row.Range,
            "{birth_date}",
            person.DateOfBirth?.ToString("dd.MM.yyyy") ?? "");

        ReplaceInRange(row.Range,
            "{Документ}",
            person.Passport?.Length > 9
                ? person.Passport[..9]
                : person.Passport ?? "");

        ReplaceInRange(row.Range,
            "{член}", "член сім'ї");

        ReplaceInRange(row.Range, "{FamilyRow}", "");
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
