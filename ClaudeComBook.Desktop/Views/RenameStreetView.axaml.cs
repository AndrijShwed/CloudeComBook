using Avalonia.Controls;
using Avalonia.Interactivity;
using ClaudeComBook.Desktop.Models;
using ClaudeComBook.Desktop.Services;
using System.Linq;

namespace ClaudeComBook.Desktop.Views;

public partial class RenameStreetView : Window
{
    private readonly Window _previousWindow;
    private readonly ApiService _api = new();
    private byte[]? _fileData;
    private bool _manualClose = false;

    public RenameStreetView(Window previousWindow)
    {
        InitializeComponent();
        UserLabel.Text = AppSession.CurrentUser?.FullName ?? AppSession.CurrentUser?.Login ?? "";
        _previousWindow = previousWindow;
        LoadData();
        RenameDateBox.AddHandler(TextInputEvent, OnDateInput, RoutingStrategies.Tunnel);
        RenameDateBox.TextChanged += OnDateTextChanged;
        VillageBox.SelectionChanged += OnVillageChanged;
        NewStreetBox.TextChanged += OnNameTextChanged;
    }

    private async void LoadData()
    {
        var villages = await _api.GetVillagesAsync();
        VillageBox.ItemsSource = villages;
        VillageBox.DisplayMemberBinding = new Avalonia.Data.Binding("Name");
    }

    private async void OnVillageChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (VillageBox.SelectedItem is not Village village) return;

        var villageStreets = await _api.GetVillageStreetsAsync();
        var streetIds = villageStreets?
            .Where(vs => vs.VillageId == village.Id && vs.IsActive)
            .Select(vs => vs.StreetId)
            .ToList();

        var allStreets = await _api.GetStreetsAsync();
        var filteredStreets = allStreets?
            .Where(s => streetIds != null && streetIds.Contains(s.Id))
            .ToList();

        OldStreetBox.ItemsSource = filteredStreets;
        OldStreetBox.DisplayMemberBinding = new Avalonia.Data.Binding("Name");
    }

    private void OnDateInput(object? sender, Avalonia.Input.TextInputEventArgs e)
    {
        if (!char.IsDigit(e.Text![0]))
            e.Handled = true;
    }

    private bool _isApplyingMask = false;
    private void OnDateTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (_isApplyingMask) return;
        _isApplyingMask = true;
        var text = RenameDateBox.Text ?? "";
        var digits = new string(text.Where(char.IsDigit).ToArray());
        if (digits.Length >= 2) digits = digits.Insert(2, ".");
        if (digits.Length >= 5) digits = digits.Insert(5, ".");
        if (digits.Length > 10) digits = digits.Substring(0, 10);
        RenameDateBox.Text = digits;
        RenameDateBox.CaretIndex = RenameDateBox.Text.Length;
        _isApplyingMask = false;
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

    private async void OnSelectFileClick(object sender, RoutedEventArgs e)
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
            FilePathBox.Text = file.Path.LocalPath;
            await using var stream = await file.OpenReadAsync();
            using var ms = new System.IO.MemoryStream();
            await stream.CopyToAsync(ms);
            _fileData = ms.ToArray();
        }
    }

    private async void OnSaveClick(object sender, RoutedEventArgs e)
    {
        if (!AppSession.IsUser)
        {
            var err = MsBox.Avalonia.MessageBoxManager
                .GetMessageBoxStandard("Доступ заборонено", "У вас немає прав для додавання!");
            err.ShowAsync();
            return;
        }
        // Перевірка обов'язкових полів
        if (VillageBox.SelectedItem == null)
        {
            var err = MsBox.Avalonia.MessageBoxManager
                .GetMessageBoxStandard("Помилка", "Виберіть населений пункт!");
            await err.ShowAsync();
            return;
        }
        if (OldStreetBox.SelectedItem == null)
        {
            var err = MsBox.Avalonia.MessageBoxManager
                .GetMessageBoxStandard("Помилка", "Виберіть стару назву вулиці!");
            await err.ShowAsync();
            return;
        }
        if (string.IsNullOrEmpty(NewStreetBox.Text))
        {
            var err = MsBox.Avalonia.MessageBoxManager
                .GetMessageBoxStandard("Помилка", "Введіть нову назву вулиці!");
            await err.ShowAsync();
            return;
        }
        if (string.IsNullOrEmpty(RenameDateBox.Text))
        {
            var err = MsBox.Avalonia.MessageBoxManager
                .GetMessageBoxStandard("Помилка", "Вкажіть дату зміни назви!");
            await err.ShowAsync();
            return;
        }

        var selectedVillage = VillageBox.SelectedItem as Village;
        var oldStreet = OldStreetBox.SelectedItem as Street;

        // Перевіряємо чи нова вулиця вже існує
        var allStreets = await _api.GetStreetsAsync();
        var newStreet = allStreets?.FirstOrDefault(s =>
            s.Name?.ToLower() == NewStreetBox.Text.ToLower());

        int newStreetId;
        if (newStreet == null)
            newStreetId = await _api.CreateStreetAsync(NewStreetBox.Text);
        else
            newStreetId = newStreet.Id;

        // Парсимо дату
        System.DateTime? renameDate = null;
        if (System.DateTime.TryParseExact(RenameDateBox.Text, "dd.MM.yyyy",
            null, System.Globalization.DateTimeStyles.None, out var date))
            renameDate = date;

        // Оновлюємо villagestreet
        await _api.RenameStreetAsync(
            selectedVillage!.Id,
            oldStreet!.Id,
            newStreetId,
            renameDate,
            _fileData);

        var msg = MsBox.Avalonia.MessageBoxManager
            .GetMessageBoxStandard("Успіх", "Вулицю перейменовано!");
        await msg.ShowAsync();

        // Очищаємо поля
        VillageBox.SelectedIndex = -1;
        OldStreetBox.SelectedIndex = -1;
        NewStreetBox.Text = "";
        RenameDateBox.Text = "";
        FilePathBox.Text = "";
        _fileData = null;
    }

    private void OnHomeClick(object sender, Avalonia.Input.TappedEventArgs e)
    {
        _manualClose = true;
        _previousWindow.Show();
        Close();
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (!_manualClose)
            _previousWindow.Show();
        base.OnClosing(e);
    }
}