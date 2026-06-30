using Avalonia.Controls;
using ClaudeComBook.Desktop.Models;
using ClaudeComBook.Desktop.Services;
using System.Collections.Generic;
using System.Linq;

namespace ClaudeComBook.Desktop.Views;

public partial class PopulationView : Window
{
    private readonly Window _previousWindow;
    private readonly ApiService _api = new();
    private bool _manualClose = false;
    private List<string> _villageNames = new();


    public PopulationView(Window previousWindow)
    {
        InitializeComponent();
        _previousWindow = previousWindow;
        PopulationGrid.LoadingRow += OnLoadingRow;
        LoadData();
    }

    private async void LoadData()
    {
        var snapshots = await _api.GetPopulationSnapshotsAsync();
        if (snapshots == null) return;

        // Збираємо унікальні населені пункти
        _villageNames = snapshots
            .Select(s => s.SettlementName ?? "")
            .Where(n => !string.IsNullOrEmpty(n))
            .Distinct()
            .OrderBy(n => n)
            .ToList();

        // Будуємо колонки динамічно
        BuildColumns();

        // Групуємо по роках
        var years = snapshots.Select(s => s.Year ?? 0).Distinct().OrderBy(y => y);
        var rows = new List<PopulationRow>();

        foreach (var year in years)
        {
            var row = new PopulationRow { Year = year };
            var yearData = snapshots.Where(s => s.Year == year);

            foreach (var village in _villageNames)
            {
                var pop = yearData.FirstOrDefault(s => s.SettlementName == village)?.Population ?? 0;
                row.VillagePopulations[village] = pop;
            }

            row.Total = row.VillagePopulations.Values.Sum();
            rows.Add(row);
        }

        PopulationGrid.ItemsSource = rows;
    }

    private void BuildColumns()
    {
        PopulationGrid.Columns.Clear();

        PopulationGrid.Columns.Add(new DataGridTextColumn
        {
            Header = "Рік",
            Binding = new Avalonia.Data.Binding("Year"),
            Width = new DataGridLength(100)
        });

        foreach (var village in _villageNames)
        {
            PopulationGrid.Columns.Add(new DataGridTextColumn
            {
                Header = village,
                Binding = new Avalonia.Data.Binding($"VillagePopulations[{village}]"),
                Width = new DataGridLength(150)
            });
        }

        PopulationGrid.Columns.Add(new DataGridTextColumn
        {
            Header = "Всього",
            Binding = new Avalonia.Data.Binding("Total"),
            Width = new DataGridLength(120)
        });
    }

    private void OnRefreshClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        LoadData();
    }

    private async void OnSaveClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var msg = MsBox.Avalonia.MessageBoxManager
            .GetMessageBoxStandard("Інфо", "Функція збереження буде додана пізніше");
        await msg.ShowAsync();
    }

    private void OnHomeClick(object sender, Avalonia.Input.TappedEventArgs e)
    {
        _manualClose = true;
        _previousWindow.Show();
        Close();
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (!_manualClose)
            _previousWindow.Show();
        base.OnClosing(e);
    }
    private void OnLoadingRow(object? sender, DataGridRowEventArgs e)
    {
        if (e.Row.DataContext is PopulationRow row)
        {
            if (row.Year == System.DateTime.Now.Year)
            {
                e.Row.Background = new Avalonia.Media.SolidColorBrush(
                    Avalonia.Media.Color.FromRgb(255, 182, 193));
                e.Row.FontWeight = Avalonia.Media.FontWeight.Bold;
            }
            else
            {
                e.Row.Background = Avalonia.Media.Brushes.Transparent;
                e.Row.FontWeight = Avalonia.Media.FontWeight.Normal;
            }
        }
    }
}