using Avalonia.Controls;
using Avalonia.Interactivity;
using ClaudeComBook.Desktop.Services;
using System.IO;
using System.Threading.Tasks;

namespace ClaudeComBook.Desktop.Views;

public partial class TemplatesManagementView : Window
{
    private readonly ApiService _api = new();

    public TemplatesManagementView()
    {
        InitializeComponent();
        CheckTemplates();
    }

    private async void CheckTemplates()
    {
        await CheckStatus("family_composition", FamilyStatus, FamilyBtn);
        await CheckStatus("characteristic", CharacteristicStatus, CharacteristicBtn);
        await CheckStatus("testament", TestamentStatus, TestamentBtn);
        await CheckStatus("subsidy", SubsidyStatus, SubsidyBtn);
        await CheckStatus("benefits", BenefitsStatus, BenefitsBtn);
        await CheckStatus("testament_registration", TestamentRegStatus, TestamentRegBtn);
    }

    private async Task CheckStatus(string type, TextBlock statusLabel, Button uploadBtn)
    {
        bool exists = await _api.TemplateExistsAsync(type);

        if (exists)
        {
            statusLabel.Text = "✅ Завантажено";
            statusLabel.Foreground = Avalonia.Media.Brushes.LightGreen;
            uploadBtn.Content = "Замінити";
            uploadBtn.Background = new Avalonia.Media.SolidColorBrush(
                Avalonia.Media.Color.FromRgb(255, 152, 0));
        }
        else
        {
            statusLabel.Text = "❌ Не завантажено";
            statusLabel.Foreground = Avalonia.Media.Brushes.Orange;
            uploadBtn.Content = "Завантажити";
            uploadBtn.Background = new Avalonia.Media.SolidColorBrush(
                Avalonia.Media.Color.FromRgb(76, 175, 80));
        }
    }

    private async void OnUploadFamilyClick(object sender, RoutedEventArgs e) =>
        await UploadTemplate("family_composition", "Довідка про склад сім'ї", FamilyStatus, FamilyBtn);

    private async void OnUploadCharacteristicClick(object sender, RoutedEventArgs e) =>
        await UploadTemplate("characteristic", "Характеристика", CharacteristicStatus, CharacteristicBtn);

    private async void OnUploadTestamentClick(object sender, RoutedEventArgs e) =>
        await UploadTemplate("testament", "Заповіт", TestamentStatus, TestamentBtn);

    private async void OnUploadSubsidyClick(object sender, RoutedEventArgs e) =>
        await UploadTemplate("subsidy", "Довідка на субсидію", SubsidyStatus, SubsidyBtn);

    private async void OnUploadBenefitsClick(object sender, RoutedEventArgs e) =>
        await UploadTemplate("benefits", "Довідка на пільги", BenefitsStatus, BenefitsBtn);

    private async void OnUploadTestamentRegClick(object sender, RoutedEventArgs e) =>
        await UploadTemplate("testament_registration", "Заява на реєстрацію заповіту", TestamentRegStatus, TestamentRegBtn);

    //private async System.Threading.Tasks.Task UploadTemplate(string type, string name, TextBlock statusLabel, Button uploadBtn)
    //{
    //    var dialog = new Avalonia.Platform.Storage.FilePickerOpenOptions
    //    {
    //        Title = $"Оберіть шаблон — {name}",
    //        AllowMultiple = false,
    //        FileTypeFilter = new[]
    //        {
    //        new Avalonia.Platform.Storage.FilePickerFileType("Word документи")
    //        {
    //            Patterns = new[] { "*.docx" }
    //        }
    //    }
    //    };

    //    var files = await StorageProvider.OpenFilePickerAsync(dialog);
    //    if (files.Count == 0) return;


    //    string currentDirectory = Directory.GetCurrentDirectory();

    //    string filePath = Path.Combine(currentDirectory, "DocTemplates", name + ".docx");

    //    //var filePath = files[0].Path.LocalPath;

    //    // Зберігаємо тільки шлях в БД
    //    await _api.SaveTemplatePathAsync(name, type, filePath);

    //    await CheckStatus(type, statusLabel, uploadBtn);

    //    var msg = MsBox.Avalonia.MessageBoxManager
    //        .GetMessageBoxStandard("Успіх", $"Шаблон \"{name}\" зареєстровано!\nШлях: {filePath}");
    //    await msg.ShowAsync();
    //}

    private async Task UploadTemplate(
    string type,
    string name,
    TextBlock statusLabel,
    Button uploadBtn)
    {
        var dialog = new Avalonia.Platform.Storage.FilePickerOpenOptions
        {
            Title = $"Оберіть шаблон — {name}",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new Avalonia.Platform.Storage.FilePickerFileType("Word документи")
            {
                Patterns = ["*.docx"]
            }
            ]
        };

        var files = await StorageProvider.OpenFilePickerAsync(dialog);

        if (files.Count == 0)
            return;

        string selectedFile = files[0].Path.LocalPath;

        await _api.UploadTemplateAsync(name, type, selectedFile);

        await CheckStatus(type, statusLabel, uploadBtn);

        await MsBox.Avalonia.MessageBoxManager
            .GetMessageBoxStandard(
                "Успіх",
                $"Шаблон \"{name}\" успішно завантажено.")
            .ShowAsync();
    }

    private void OnCloseClick(object sender, RoutedEventArgs e)
    {
        Close();
    }
}