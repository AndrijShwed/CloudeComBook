using Avalonia.Controls;

namespace ClaudeComBook.Desktop.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnVillagesClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var window = new VillagesView();
            window.Show();
        }

        private void OnPeopleClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var window = new PeopleView();
            window.Show();
        }
    }
}