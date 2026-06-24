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
            var window = new VillageStreetsView(this);
            window.Show();
            this.Hide();
        }
        private void OnPeopleClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var window = new PeopleView(this);
            window.Show();
            this.Hide();
        }

        private void OnRenameStreetClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var window = new RenameStreetView(this);
            window.Show();
            this.Hide();
        }
    }
}