using Avalonia.Controls;

namespace ClaudeComBook.Desktop.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnVillagesStreetsClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
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

        private void OnPopulationClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var window = new PopulationView(this);
            window.Show();
            this.Hide();
        }
        private void OnHouseholdsClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var window = new HouseholdsView(this);
            window.Show();
            this.Hide();
        }
        private void OnEnterprisesClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var window = new EnterprisesView(this);
            window.Show();
            this.Hide();
        }
        private void OnAnimalsClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var window = new AnymalsView(this);
            window.Show();
            this.Hide();
        }
    }
}