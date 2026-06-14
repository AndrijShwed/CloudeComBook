using Avalonia.Controls;
using ClaudeComBook.Desktop.Services;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace ClaudeComBook.Desktop.Views;

public partial class PeopleSearchView : Window
{
    private readonly ApiService _api = new();

    public PeopleSearchView()
    {
        InitializeComponent();
        LoadComboBoxes();
        AgeFromBox.AddHandler(TextInputEvent, OnAgeInput, RoutingStrategies.Tunnel);
        AgeToBox.AddHandler(TextInputEvent, OnAgeInput, RoutingStrategies.Tunnel);
    }

    private void OnAgeInput(object sender, TextInputEventArgs e)
    {
        e.Handled = !int.TryParse(e.Text, out _);
    }

    private async void LoadComboBoxes()
    {
        var villages = await _api.GetVillagesAsync();
        VillageBox.ItemsSource = villages;
        VillageBox.DisplayMemberBinding = new Avalonia.Data.Binding("Name");

        var streets = await _api.GetStreetsAsync();
        StreetBox.ItemsSource = streets;
        StreetBox.DisplayMemberBinding = new Avalonia.Data.Binding("Name");
    }

    private async void OnSearchClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        int ageFrom = 0, ageTo = 200;

        if (!string.IsNullOrEmpty(AgeFromBox.Text))
        {
            if (!int.TryParse(AgeFromBox.Text, out ageFrom) || ageFrom > 200)
            {
                AgeFromBox.Text = "";
                var msg = MsBox.Avalonia.MessageBoxManager
                    .GetMessageBoxStandard("Помилка", "Вік від: має бути від 0 до 200");
                await msg.ShowAsync();
                return;
            }
        }

        if (!string.IsNullOrEmpty(AgeToBox.Text))
        {
            if (!int.TryParse(AgeToBox.Text, out ageTo) || ageTo > 200)
            {
                AgeToBox.Text = "";
                var msg = MsBox.Avalonia.MessageBoxManager
                    .GetMessageBoxStandard("Помилка", "Вік до: має бути від 0 до 200");
                await msg.ShowAsync();
                return;
            }
        }

        if (ageFrom > ageTo)
        {
            AgeFromBox.Text = "";
            AgeToBox.Text = "";
            var msg = MsBox.Avalonia.MessageBoxManager
                .GetMessageBoxStandard("Помилка", "Вік від: не може бути більше ніж Вік до:");
            await msg.ShowAsync();
            return;
        }

        var people = await _api.GetPeopleAsync();
        PeopleGrid.ItemsSource = people;
        ResultCount.Text = people?.Count.ToString() ?? "0";
    }

    private void OnClearClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        LastNameBox.Text = "";
        FirstNameBox.Text = "";
        SurnameBox.Text = "";
        HouseBox.Text = "";
        AgeFromBox.Text = "";
        AgeToBox.Text = "";
        SexBox.SelectedIndex = -1;
        VillageBox.SelectedIndex = -1;
        StreetBox.SelectedIndex = -1;
        StatusBox.SelectedIndex = -1;
        RegistrYesBox.IsChecked = true;
        RegistrNoBox.IsChecked = false;
    }

    private void OnClearTableClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        PeopleGrid.ItemsSource = null;
        ResultCount.Text = "0";
    }

    private void OnExcelClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        // Пізніше додамо експорт в Excel
    }
}