using Avalonia.Controls;
using Avalonia.Interactivity;
using ClaudeComBook.Desktop.Services;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace ClaudeComBook.Desktop.Views;

public partial class HouseAreaView : Window
{
    private readonly Window _previousWindow;
    private readonly ApiService _api = new();
    private bool _manualClose = false;

    public HouseAreaView(Window previousWindow)
    {
        InitializeComponent();
        UserLabel.Text = AppSession.CurrentUser?.FullName ?? AppSession.CurrentUser?.Login ?? "";
        _previousWindow = previousWindow;
        LoadData();
    }

    private async void LoadData()
    {
        var response = await _api.GetAreaByVillageAsync();
        if (response == null) return;

        // Збираємо унікальні села
        var villages = response.Select(r => r.Village).Distinct().OrderBy(v => v).ToList();

        // Будуємо колонки
        AreaGrid.Columns.Clear();
        AreaGrid.Columns.Add(new DataGridTextColumn
        {
            Header = "Рік",
            Binding = new Avalonia.Data.Binding("Year"),
            Width = new DataGridLength(80)
        });

        foreach (var village in villages)
        {
            AreaGrid.Columns.Add(new DataGridTextColumn
            {
                Header = $"{village}\nзаг.пл.",
                Binding = new Avalonia.Data.Binding($"TotalAreas[{village}]"),
                Width = new DataGridLength(120)
            });
            AreaGrid.Columns.Add(new DataGridTextColumn
            {
                Header = $"{village}\nжитл.пл.",
                Binding = new Avalonia.Data.Binding($"LivingAreas[{village}]"),
                Width = new DataGridLength(120)
            });
        }

        AreaGrid.Columns.Add(new DataGridTextColumn
        {
            Header = "Всього заг. пл.",
            Binding = new Avalonia.Data.Binding("TotalAreaSum"),
            Width = new DataGridLength(130)
        });
        AreaGrid.Columns.Add(new DataGridTextColumn
        {
            Header = "Всього житл. пл.",
            Binding = new Avalonia.Data.Binding("LivingAreaSum"),
            Width = new DataGridLength(130)
        });

        // Створюємо один рядок з поточними даними
        var row = new AreaRow
        {
            Year = System.DateTime.Now.Year
        };

        foreach (var item in response)
        {
            row.TotalAreas[item.Village] = item.TotalArea;
            row.LivingAreas[item.Village] = item.LivingArea;
        }

        row.TotalAreaSum = row.TotalAreas.Values.Sum();
        row.LivingAreaSum = row.LivingAreas.Values.Sum();

        AreaGrid.ItemsSource = new List<AreaRow> { row };
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

public class AreaRow
{
    public int Year { get; set; }
    public Dictionary<string, decimal> TotalAreas { get; set; } = new();
    public Dictionary<string, decimal> LivingAreas { get; set; } = new();
    public decimal TotalAreaSum { get; set; }
    public decimal LivingAreaSum { get; set; }
}