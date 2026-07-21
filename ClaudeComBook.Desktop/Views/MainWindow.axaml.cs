using Avalonia.Controls;
using ClaudeComBook.Desktop.Services;

namespace ClaudeComBook.Desktop.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            UserLabel.Text = AppSession.CurrentUser?.FullName ?? AppSession.CurrentUser?.Login ?? "";

            // Показуємо кнопки тільки для адміна
            if (AppSession.IsAdmin)
            {
                UsersBtn.IsVisible = true;
                AdminPanelBtn.IsVisible = true;
            }
        }

        private void OnUsersClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var window = new UsersManagementView(this);
            window.Show();
            this.Hide();
        }

        private void OnAdminPanelClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var window = new UsersManagementView(this);
            window.Show();
            this.Hide();
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
        private void OnLandClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var window = new LandView(this);
            window.Show();
            this.Hide();
        }
    }
}