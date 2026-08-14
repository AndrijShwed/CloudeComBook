using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using CloudeComBook.Desktop.Services;
using System.Threading.Tasks;

namespace CloudeComBook.Desktop.Views;

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

    private async Task CheckStatus(
    string type,
    TextBlock statusLabel,
    Button uploadButton)
    {
        bool exists = await _api.TemplateExistsAsync(type);

        if (exists)
        {
            statusLabel.Text = "✅ Завантажено";
            statusLabel.Foreground = Brushes.LightGreen;

            uploadButton.Content = "Замінити";
            uploadButton.Background =
                new SolidColorBrush(Color.FromRgb(255, 152, 0));
        }
        else
        {
            statusLabel.Text = "❌ Не завантажено";
            statusLabel.Foreground = Brushes.Orange;

            uploadButton.Content = "Завантажити";
            uploadButton.Background =
                new SolidColorBrush(Color.FromRgb(76, 175, 80));
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

    private async Task UploadTemplate(
     string type,
     string templateName,
     TextBlock statusLabel,
     Button uploadButton)
    {
        var files = await StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                Title = $"Виберіть шаблон \"{templateName}\"",
                AllowMultiple = false,
                FileTypeFilter =
                [
                    new FilePickerFileType("Документи Word")
                {
                    Patterns = new[] { "*.docx" },
                    MimeTypes = new[] { "application/vnd.openxmlformats-officedocument.wordprocessingml.document" }
                }
                ]
            });

        if (files.Count == 0)
            return;

        string? filePath = files[0].TryGetLocalPath();

        if (string.IsNullOrWhiteSpace(filePath))
            return;

        await _api.UploadTemplateAsync(type, filePath);

        var msg = MsBox.Avalonia.MessageBoxManager
                .GetMessageBoxStandard(
                    "Успішно",
                    "Шаблон успішно замінено.");

        await msg.ShowAsync();

        statusLabel.Text = "✅ Завантажено";
        statusLabel.Foreground = Brushes.LightGreen;

        uploadButton.Content = "Замінити";
        uploadButton.Background =
            new SolidColorBrush(Color.FromRgb(255, 152, 0));
    }
    private void OnCloseClick(object sender, RoutedEventArgs e)
    {
        Close();
    }
}