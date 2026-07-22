using Avalonia.Controls;
using Avalonia.Interactivity;
using ClaudeComBook.Desktop.Services;
using System.Collections.Generic;
using System.Linq;

namespace ClaudeComBook.Desktop.Views;

public partial class AnymalStatisticsView : Window
{
    private readonly Window _previousWindow;
    private readonly ApiService _api = new();
    private bool _manualClose = false;

    public AnymalStatisticsView(Window previousWindow)
    {
        InitializeComponent();
        UserLabel.Text = AppSession.CurrentUser?.FullName ?? AppSession.CurrentUser?.Login ?? "";
        _previousWindow = previousWindow;
        LoadData();
        StatisticsGrid.LoadingRow += OnLoadingRow;
    }

    private async void LoadData()
    {
        var data = await _api.GetAnymalStatisticsAsync();
        if (data == null) return;

        var rows = new List<AnymalStatRow>();
        foreach (var item in data)
        {
            rows.Add(new AnymalStatRow
            {
                Village = item.Village,
                Covs = item.Covs,
                Pigs = item.Pigs,
                Sheeps = item.Sheeps,
                Goats = item.Goats,
                Horses = item.Horses,
                Birds = item.Birds,
                Rabbits = item.Rabbits,
                Beeses = item.Beeses,
                Anymals = item.Anymals
            });
        }
        // Додаємо рядок з підсумками
        var totalRow = new AnymalStatRow
        {
            Village = "Загальні",
            Anymals = rows.Sum(r => r.Anymals),
            Covs = rows.Sum(r => r.Covs),
            Pigs = rows.Sum(r => r.Pigs),
            Sheeps = rows.Sum(r => r.Sheeps),
            Goats = rows.Sum(r => r.Goats),
            Horses = rows.Sum(r => r.Horses),
            Birds = rows.Sum(r => r.Birds),
            Rabbits = rows.Sum(r => r.Rabbits),
            Beeses = rows.Sum(r => r.Beeses)
        };
        rows.Add(totalRow);

        StatisticsGrid.ItemsSource = rows;
    }

    private void OnRefreshClick(object sender, RoutedEventArgs e)
    {
        LoadData();
    }

    private void OnHomeClick(object sender, Avalonia.Input.TappedEventArgs e)
    {
        _manualClose = true;
        if (_previousWindow is AnymalsView animalsView)
            animalsView._previousWindow.Show();
        _previousWindow.Close();
        Close();
    }

    private void OnAnymalsClick(object sender, Avalonia.Input.TappedEventArgs e)
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
    private void OnLoadingRow(object? sender, DataGridRowEventArgs e)
    {
        if (e.Row.DataContext is AnymalStatRow row && row.Village == "Загальні")
        {
            e.Row.FontWeight = Avalonia.Media.FontWeight.Bold;
            e.Row.Background = new Avalonia.Media.SolidColorBrush(
                Avalonia.Media.Color.FromRgb(255, 200, 0));
        }
        else
        {
            e.Row.FontWeight = Avalonia.Media.FontWeight.Normal;
            e.Row.Background = Avalonia.Media.Brushes.Transparent;
        }
    }
}

public class AnymalStatRow
{
    public string? Village { get; set; }
    public int Covs { get; set; }
    public int Pigs { get; set; }
    public int Sheeps { get; set; }
    public int Goats { get; set; }
    public int Horses { get; set; }
    public int Birds { get; set; }
    public int Rabbits { get; set; }
    public int Beeses { get; set; }
    public int Anymals { get; set; }
}