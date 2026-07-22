using Avalonia.Controls;
using Avalonia.Interactivity;
using ClaudeComBook.Desktop.Services;
using System.Collections.Generic;

namespace ClaudeComBook.Desktop.Views;

public partial class HouseRoomsView : Window
{
    private readonly Window _previousWindow;
    private readonly ApiService _api = new();
    private bool _manualClose = false;

    public HouseRoomsView(Window previousWindow)
    {
        InitializeComponent();
        UserLabel.Text = AppSession.CurrentUser?.FullName ?? AppSession.CurrentUser?.Login ?? "";
        _previousWindow = previousWindow;
        LoadData();
    }

    private async void LoadData()
    {
        var data = await _api.GetRoomsByVillageAsync();
        if (data == null) return;

        var year = System.DateTime.Now.Year;
        var rows = new List<RoomsRow>();

        foreach (var item in data)
        {
            rows.Add(new RoomsRow
            {
                Village = item.Village,
                Year = year,
                OneRoom = item.OneRoom,
                TwoRooms = item.TwoRooms,
                ThreeRooms = item.ThreeRooms,
                FourRooms = item.FourRooms,
                FiveRooms = item.FiveRooms,
                SixRooms = item.SixRooms,
                MoreThanSix = item.MoreThanSix,
                Total = item.Total
            });
        }

        RoomsGrid.ItemsSource = rows;
    }

    private void OnRefreshClick(object sender, RoutedEventArgs e)
    {
        LoadData();
    }

    private void OnHomeClick(object sender, Avalonia.Input.TappedEventArgs e)
    {
        _manualClose = true;
        if (_previousWindow is HouseholdsView householdsView)
            householdsView._previousWindow.Show();
        _previousWindow.Close();
        Close();
    }

    private void OnHouseholdsClick(object sender, Avalonia.Input.TappedEventArgs e)
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

public class RoomsRow
{
    public string? Village { get; set; }
    public int Year { get; set; }
    public int OneRoom { get; set; }
    public int TwoRooms { get; set; }
    public int ThreeRooms { get; set; }
    public int FourRooms { get; set; }
    public int FiveRooms { get; set; }
    public int SixRooms { get; set; }
    public int MoreThanSix { get; set; }
    public int Total { get; set; }
}