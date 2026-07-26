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
        await CheckStatus("family_composition", FamilyStatus, FamilyBtn);
        await CheckStatus("characteristic", CharacteristicStatus, CharacteristicBtn);
        await CheckStatus("testament", TestamentStatus, TestamentBtn);
        await CheckStatus("subsidy", SubsidyStatus, SubsidyBtn);
        await CheckStatus("benefits", BenefitsStatus, BenefitsBtn);
        await CheckStatus("testament_registration", TestamentRegStatus, TestamentRegBtn);
    }

    private async System.Threading.Tasks.Task CheckStatus(string type, TextBlock statusLabel, Button uploadBtn)
    {
        var template = await _api.GetTemplateByTypeAsync(type);
        if (template != null)
        {
            statusLabel.Text = "✅ Завантажено";
            statusLabel.Foreground = Avalonia.Media.Brushes.LightGreen;
            uploadBtn.Content = "Замінити";
            uploadBtn.Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(255, 152, 0));
        }
        else
        {
            statusLabel.Text = "❌ Не завантажено";
            statusLabel.Foreground = Avalonia.Media.Brushes.Orange;
            uploadBtn.Content = "Завантажити";
            uploadBtn.Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(76, 175, 80));
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

    private async System.Threading.Tasks.Task UploadTemplate(string type, string name, TextBlock statusLabel, Button uploadBtn)
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

        // Оновлюємо статус
        await CheckStatus(type, statusLabel, uploadBtn);

        var msg = MsBox.Avalonia.MessageBoxManager
            .GetMessageBoxStandard("Успіх", $"Шаблон \"{name}\" завантажено!");
        await msg.ShowAsync();
    }

    private void OnCloseClick(object sender, RoutedEventArgs e)
    {
        Close();
    }
}