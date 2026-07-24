using Avalonia.Controls;
using Avalonia.Interactivity;
using ClaudeComBook.Desktop.Services;

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
        await CheckStatus("family_composition", FamilyStatus);
        await CheckStatus("characteristic", CharacteristicStatus);
        await CheckStatus("testament", TestamentStatus);
        await CheckStatus("subsidy", SubsidyStatus);
        await CheckStatus("benefits", BenefitsStatus);
        await CheckStatus("testament_registration", TestamentRegStatus);
    }

    private async System.Threading.Tasks.Task CheckStatus(string type, TextBlock statusLabel)
    {
        var template = await _api.GetTemplateByTypeAsync(type);
        if (template != null)
        {
            statusLabel.Text = "✅ Завантажено";
            statusLabel.Foreground = Avalonia.Media.Brushes.LightGreen;
        }
        else
        {
            statusLabel.Text = "❌ Не завантажено";
            statusLabel.Foreground = Avalonia.Media.Brushes.Orange;
        }
    }

    private async void OnUploadFamilyClick(object sender, RoutedEventArgs e) =>
        await UploadTemplate("family_composition", "Довідка про склад сім'ї", FamilyStatus);

    private async void OnUploadCharacteristicClick(object sender, RoutedEventArgs e) =>
        await UploadTemplate("characteristic", "Характеристика", CharacteristicStatus);

    private async void OnUploadTestamentClick(object sender, RoutedEventArgs e) =>
        await UploadTemplate("testament", "Заповіт", TestamentStatus);

    private async void OnUploadSubsidyClick(object sender, RoutedEventArgs e) =>
        await UploadTemplate("subsidy", "Довідка на субсидію", SubsidyStatus);

    private async void OnUploadBenefitsClick(object sender, RoutedEventArgs e) =>
        await UploadTemplate("benefits", "Довідка на пільги", BenefitsStatus);

    private async void OnUploadTestamentRegClick(object sender, RoutedEventArgs e) =>
        await UploadTemplate("testament_registration", "Заява на реєстрацію заповіту", TestamentRegStatus);

    private async System.Threading.Tasks.Task UploadTemplate(string type, string name, TextBlock statusLabel)
    {
        var dialog = new Avalonia.Platform.Storage.FilePickerOpenOptions
        {
            Title = $"Оберіть шаблон — {name}",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new Avalonia.Platform.Storage.FilePickerFileType("Word документи")
                {
                    Patterns = new[] { "*.docx" }
                }
            }
        };

        var files = await StorageProvider.OpenFilePickerAsync(dialog);
        if (files.Count == 0) return;

        var file = files[0];
        await using var stream = await file.OpenReadAsync();
        using var ms = new System.IO.MemoryStream();
        await stream.CopyToAsync(ms);
        var fileBytes = ms.ToArray();

        await _api.UploadTemplateAsync(name, type, fileBytes);

        statusLabel.Text = "✅ Завантажено";
        statusLabel.Foreground = Avalonia.Media.Brushes.LightGreen;

        var msg = MsBox.Avalonia.MessageBoxManager
            .GetMessageBoxStandard("Успіх", $"Шаблон \"{name}\" завантажено!");
        await msg.ShowAsync();
    }

    private void OnCloseClick(object sender, RoutedEventArgs e)
    {
        Close();
    }
}