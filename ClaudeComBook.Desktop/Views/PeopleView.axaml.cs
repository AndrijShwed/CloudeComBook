using Avalonia.Controls;

namespace ClaudeComBook.Desktop.Views;

public partial class PeopleView : Window
{
    public PeopleView()
    {
        InitializeComponent();
    }

    private void OnAddClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        // Пізніше відкриємо вікно додавання
    }

    private void OnOpenSearchWindowClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var window = new PeopleSearchView();
        window.Show();
    }
}