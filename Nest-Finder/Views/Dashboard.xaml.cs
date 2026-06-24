using System.Windows;
using System.Windows.Controls;
using NestFinder.Helpers;
using NestFinder.Services;

namespace NestFinder.Views
{
    public partial class Dashboard : UserControl
    {
        private readonly DashboardRepository _dashboardRepository = new();

        public Dashboard()
        {
            InitializeComponent();
            Loaded += Dashboard_Loaded;
        }

        private void Dashboard_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            var currentUser = SessionManager.CurrentUser;
            if (currentUser != null)
            {
                TxtWelcomeSubtitle.Text = $"Welcome back, {currentUser.FullName}!";
                TxtRoleBadgeText.Text = currentUser.Role;
                
                string bgPath = currentUser.Role == "Admin" ? "pack://application:,,,/assets/admin_bg.jpg" : "pack://application:,,,/assets/customer_bg.jpg";
                try
                {
                    var uri = new System.Uri(bgPath, System.UriKind.Absolute);
                    ImgDbBackground.ImageSource = new System.Windows.Media.Imaging.BitmapImage(uri);
                    ImgDbBackground.Opacity = currentUser.Role == "Admin" ? 0.22 : 0.12;
                }
                catch { }

                if (currentUser.Role == "Admin")
                {
                    UserRoleBadge.Background = (System.Windows.Media.Brush)FindResource("AccentPurpleBrush");
                }
                else
                {
                    UserRoleBadge.Background = (System.Windows.Media.Brush)FindResource("AccentTealBrush");
                    TxtRoleBadgeText.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(13, 17, 23));
                }
            }

            // Fetch statistics
            var stats = _dashboardRepository.GetStats();

            // Populate UI Elements
            TxtTotalProperties.Text = stats.TotalProperties.ToString();
            TxtAvailableProperties.Text = stats.Available.ToString();
            TxtSoldRentedProperties.Text = stats.SoldRented.ToString();

            TransactionsGrid.ItemsSource = stats.RecentTransactions;
            ViewingsItemsControl.ItemsSource = stats.UpcomingViewings;
        }
    }
}
