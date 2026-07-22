using Avalonia.Controls;
using ClaudeComBook.Desktop.Models;
using ClaudeComBook.Desktop.Services;
using System.Collections.Generic;
using System.Linq;

namespace ClaudeComBook.Desktop.Views;

public partial class VillageStreetsView : Window
{
    private readonly Window _previousWindow;
    private readonly ApiService _api = new();

    public VillageStreetsView(Window previousWindow)
    {
        InitializeComponent();
        UserLabel.Text = AppSession.CurrentUser?.FullName ?? AppSession.CurrentUser?.Login ?? "";
        _previousWindow = previousWindow;
        LoadData();
        VillageInputBox.TextChanged += OnNameTextChanged;
        StreetInputBox.TextChanged += OnNameTextChanged;
    }

    private async void LoadData()
    {
        var villageStreets = await _api.GetVillageStreetsAsync();
        VillageStreetsGrid.ItemsSource = villageStreets;
    }

    private void OnNameTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (sender is TextBox box && !string.IsNullOrEmpty(box.Text))
        {
            var text = box.Text;
            var capitalized = char.ToUpper(text[0]) + text.Substring(1);
            if (text != capitalized)
            {
                box.Text = capitalized;
                box.CaretIndex = box.Text.Length;
            }
        }
    }

    private async void OnAddClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (!AppSession.IsUser)
        {
            var err = MsBox.Avalonia.MessageBoxManager
                .GetMessageBoxStandard("Доступ заборонено", "У вас немає прав для додавання!");
            err.ShowAsync();
            return;
        }
        var villageName = VillageInputBox.Text?.Trim();
        var streetName = StreetInputBox.Text?.Trim();

        if (string.IsNullOrEmpty(villageName) || string.IsNullOrEmpty(streetName))
        {
            var msg = MsBox.Avalonia.MessageBoxManager
                .GetMessageBoxStandard("Помилка", "Введіть назву населеного пункту і вулиці!");
            await msg.ShowAsync();
            return;
        }

        // Перевіряємо чи є village
        var villages = await _api.GetVillagesAsync();
        var village = villages?.FirstOrDefault(v =>
            v.Name?.ToLower() == villageName.ToLower());

        int villageId;
        if (village == null)
        {
            villageId = await _api.CreateVillageAsync(villageName);
        }
        else
        {
            villageId = village.Id;
        }

        // Перевіряємо чи є street
        var streets = await _api.GetStreetsAsync();
        var street = streets?.FirstOrDefault(s =>
            s.Name?.ToLower() == streetName.ToLower());

        int streetId;
        if (street == null)
        {
            streetId = await _api.CreateStreetAsync(streetName);
        }
        else
        {
            streetId = street.Id;
        }

        // Додаємо в villagestreet
        await _api.CreateVillageStreetAsync(villageId, streetId);

        // Очищаємо поля і оновлюємо таблицю
        VillageInputBox.Text = "";
        StreetInputBox.Text = "";
        LoadData();
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        _previousWindow.Show();
        base.OnClosing(e);
    }

    private async void OnDeleteClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (!AppSession.IsAdmin)
        {
            var err = MsBox.Avalonia.MessageBoxManager
                .GetMessageBoxStandard("Доступ заборонено", "Тільки адміністратор може видаляти записи!");
            await err.ShowAsync();
            return;
        }
        if (sender is Button btn && btn.DataContext is VillageStreet vs)
        {
            var msg = MsBox.Avalonia.MessageBoxManager
                .GetMessageBoxStandard("Підтвердження",
                    $"Ви дійсно хочете видалити вулицю \"{vs.StreetName}\" у населеному пункті \"{vs.VillageName}\"?",
                    MsBox.Avalonia.Enums.ButtonEnum.YesNo);
            var result = await msg.ShowAsync();

            if (result == MsBox.Avalonia.Enums.ButtonResult.Yes)
            {
                await _api.DeleteVillageStreetAsync(vs.Id);
                LoadData();
            }
        }
    }

    private async void OnOpenFileClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is VillageStreet vs)
        {
            if (vs.FileData == null || vs.FileData.Length == 0) return;

            var ext = GetFileExtension(vs.FileData);
            var tempPath = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(), $"doc_{vs.Id}{ext}");
            await System.IO.File.WriteAllBytesAsync(tempPath, vs.FileData);

            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = tempPath,
                UseShellExecute = true
            });
        }
    }

    private string GetFileExtension(byte[] fileData)
    {
        // PDF: %PDF
        if (fileData.Length >= 4 &&
            fileData[0] == 0x25 && fileData[1] == 0x50 &&
            fileData[2] == 0x44 && fileData[3] == 0x46)
            return ".pdf";

        // DOCX/ZIP: PK
        if (fileData.Length >= 2 &&
            fileData[0] == 0x50 && fileData[1] == 0x4B)
            return ".docx";

        // DOC: D0 CF
        if (fileData.Length >= 2 &&
            fileData[0] == 0xD0 && fileData[1] == 0xCF)
            return ".doc";

        return ".pdf"; // за замовчуванням
    }

    private async void OnReplaceFileClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (!AppSession.IsUser)
        {
            var err = MsBox.Avalonia.MessageBoxManager
                .GetMessageBoxStandard("Доступ заборонено", "У вас немає прав для цієї дії!");
            await err.ShowAsync();
            return;
        }
        if (sender is Button btn && btn.DataContext is VillageStreet vs)
        {
            var dialog = new Avalonia.Platform.Storage.FilePickerOpenOptions
            {
                Title = "Оберіть файл",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                new Avalonia.Platform.Storage.FilePickerFileType("Документи")
                {
                    Patterns = new[] { "*.pdf", "*.doc", "*.docx" }
                }
            }
            };

            var files = await StorageProvider.OpenFilePickerAsync(dialog);
            if (files.Count > 0)
            {
                var file = files[0];
                await using var stream = await file.OpenReadAsync();
                using var ms = new System.IO.MemoryStream();
                await stream.CopyToAsync(ms);
                var fileData = ms.ToArray();

                await _api.UpdateVillageStreetFileAsync(vs.Id, fileData);
                LoadData();
            }
        }
    }

    private bool _manualClose = false;
    private void OnHomeClick(object sender, Avalonia.Input.TappedEventArgs e)
    {
        _manualClose = true;
        if (_previousWindow is PeopleView peopleView)
        {
            peopleView._previousWindow.Show();
            peopleView.Hide();
        }
        else
        {
            _previousWindow.Show();
        }
        Close();
    }
}