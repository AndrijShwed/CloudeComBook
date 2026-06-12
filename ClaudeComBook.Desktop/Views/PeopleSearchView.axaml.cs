using Avalonia.Controls;
using ClaudeComBook.Desktop.Services;

namespace ClaudeComBook.Desktop.Views;

public partial class PeopleSearchView : Window
{
    private readonly ApiService _api = new();

    public PeopleSearchView()
    {
        InitializeComponent();
        LoadComboBoxes();
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