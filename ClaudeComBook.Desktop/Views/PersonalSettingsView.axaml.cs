using Avalonia.Controls;
using Avalonia.Interactivity;
using ClaudeComBook.Desktop.Services;

namespace ClaudeComBook.Desktop.Views;

public partial class PersonalSettingsView : Window
{
    private readonly ApiService _api = new();

    public PersonalSettingsView()
    {
        InitializeComponent();
        LoadData();
    }

    private async void LoadData()
    {
        var user = await _api.GetCurrentUserAsync();
        if (user != null)
        {
            PositionBox.Text = user.Position;
            FullNameBox.Text = user.FullName;
            ShortNameBox.Text = user.ShortName;
            OrganizationBox.Text = user.Organization;
            RegionBox.Text = user.Region;
            DistrictBox.Text = user.District;
            VillageBox.Text = user.Village;
            StreetBox.Text = user.Street;
            HouseBox.Text = user.House;
            PhoneBox.Text = user.Phone;
            PostIndexBox.Text = user.PostIndex;
        }
    }

    private async void OnSaveClick(object sender, RoutedEventArgs e)
    {
        var ok = await _api.UpdatePersonalSettingsAsync(
            AppSession.CurrentUser!.Id,
            FullNameBox.Text,
            PositionBox.Text,
            ShortNameBox.Text,
            OrganizationBox.Text,
            RegionBox.Text,
            DistrictBox.Text,
            VillageBox.Text,
            StreetBox.Text,
            HouseBox.Text,
            PhoneBox.Text,
            PostIndexBox.Text);

        if (ok)
        {
            // Оновлюємо сесію
            AppSession.CurrentUser.FullName = FullNameBox.Text;
            AppSession.CurrentUser.Position = PositionBox.Text;
            AppSession.CurrentUser.ShortName = ShortNameBox.Text;

            var msg = MsBox.Avalonia.MessageBoxManager
                .GetMessageBoxStandard("Успіх", "Налаштування збережено!");
            await msg.ShowAsync();
            Close();
        }
        else
        {
            var err = MsBox.Avalonia.MessageBoxManager
                .GetMessageBoxStandard("Помилка", "Помилка збереження!");
            await err.ShowAsync();
        }
    }
}