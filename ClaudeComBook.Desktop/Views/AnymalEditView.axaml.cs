using Avalonia.Controls;
using Avalonia.Interactivity;
using ClaudeComBook.Desktop.Models;
using ClaudeComBook.Desktop.Services;
using MsBox.Avalonia.Base;
using System.Linq;

namespace ClaudeComBook.Desktop.Views;

public partial class AnymalEditView : Window
{
    private readonly Window _previousWindow;
    private readonly ApiService _api = new();
    private readonly Anymal _anymal;
    private bool _manualClose = false;

    public AnymalEditView(Anymal anymal, Window previousWindow)
    {
        InitializeComponent();
        _anymal = anymal;
        _previousWindow = previousWindow;
        LoadData();
        DeleteBtn.IsVisible = AppSession.IsAdmin;
    }

    private async void LoadData()
    {
        var villages = await _api.GetVillagesAsync();
        VillageBox.ItemsSource = villages;
        VillageBox.DisplayMemberBinding = new Avalonia.Data.Binding("Name");

        LastNameBox.Text = _anymal.LastName;
        FirstNameBox.Text = _anymal.Name;
        SurnameBox.Text = _anymal.Surname;
        AnymalsBox.Text = _anymal.Anymals?.ToString();
        CovsBox.Text = _anymal.Covs?.ToString();
        PigsBox.Text = _anymal.Pigs?.ToString();
        SheepsBox.Text = _anymal.Sheeps?.ToString();
        GoatsBox.Text = _anymal.Goats?.ToString();
        HorsesBox.Text = _anymal.Horses?.ToString();
        BirdsBox.Text = _anymal.Birds?.ToString();
        RabbitsBox.Text = _anymal.Rabbits?.ToString();
        BeesesBox.Text = _anymal.Beeses?.ToString();

        if (_anymal.Village != null && villages != null)
        {
            var village = villages.FirstOrDefault(v => v.Name == _anymal.Village);
            if (village != null)
                VillageBox.SelectedItem = village;
        }
    }

    private async void OnSaveClick(object sender, RoutedEventArgs e)
    {
        var selectedVillage = VillageBox.SelectedItem as Village;

        _anymal.LastName = LastNameBox.Text;
        _anymal.Name = FirstNameBox.Text;
        _anymal.Surname = SurnameBox.Text;
        _anymal.Village = selectedVillage?.Name ?? _anymal.Village;
        _anymal.Anymals = int.TryParse(AnymalsBox.Text, out var anymals) ? anymals : 0;
        _anymal.Covs = int.TryParse(CovsBox.Text, out var covs) ? covs : 0;
        _anymal.Pigs = int.TryParse(PigsBox.Text, out var pigs) ? pigs : 0;
        _anymal.Sheeps = int.TryParse(SheepsBox.Text, out var sheeps) ? sheeps : 0;
        _anymal.Goats = int.TryParse(GoatsBox.Text, out var goats) ? goats : 0;
        _anymal.Horses = int.TryParse(HorsesBox.Text, out var horses) ? horses : 0;
        _anymal.Birds = int.TryParse(BirdsBox.Text, out var birds) ? birds : 0;
        _anymal.Rabbits = int.TryParse(RabbitsBox.Text, out var rabbits) ? rabbits : 0;
        _anymal.Beeses = int.TryParse(BeesesBox.Text, out var beeses) ? beeses : 0;

        await _api.UpdateAnymalAsync(_anymal);

        var msg = MsBox.Avalonia.MessageBoxManager
            .GetMessageBoxStandard("Успіх", "Зміни збережено!");
        await msg.ShowAsync();

        if (_previousWindow is AnymalSearchView searchView)
            searchView.OnSearchClick(null, null);

        _previousWindow.Show();
        Close();
    }

    private async void OnDeleteClick(object sender, RoutedEventArgs e)
    {
        var msg = MsBox.Avalonia.MessageBoxManager
            .GetMessageBoxStandard("Підтвердження",
                $"Ви дійсно хочете видалити запис \"{_anymal.LastName} {_anymal.Name}\"?",
                MsBox.Avalonia.Enums.ButtonEnum.YesNo);
        var result = await msg.ShowAsync();

        if (result == MsBox.Avalonia.Enums.ButtonResult.Yes)
        {
            await _api.DeleteAnymalAsync(_anymal.AnymalsId);
            if (_previousWindow is AnymalSearchView searchView)
                searchView.OnSearchClick(null, null);
            _previousWindow.Show();
            Close();
        }
    }

    private void OnBackClick(object sender, RoutedEventArgs e)
    {
        _previousWindow.Show();
        Close();
    }

    private void OnBackClick2(object sender, Avalonia.Input.TappedEventArgs e)
    {
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