using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using NestFinder.Helpers;
using NestFinder.Views;

namespace NestFinder
{
    public partial class MainWindow : MetroWindow
    {
        public static readonly DependencyProperty CurrentViewProperty =
            DependencyProperty.Register("CurrentView", typeof(object), typeof(MainWindow), new PropertyMetadata(null));

        public object CurrentView
        {
            get { return GetValue(CurrentViewProperty); }
            set { SetValue(CurrentViewProperty, value); }
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var currentUser = SessionManager.CurrentUser;
            if (currentUser == null)
            {
                // Safety check: redirect to Login
                var loginWindow = new LoginWindow();
                loginWindow.Show();
                this.Close();
                return;
            }

            // Set dynamic background image
            string bgPath = currentUser.Role == "Admin" ? "pack://application:,,,/assets/admin_bg.jpg" : "pack://application:,,,/assets/customer_bg.jpg";
            try
            {
                var uri = new System.Uri(bgPath, System.UriKind.Absolute);
                ImgMainBackground.ImageSource = new System.Windows.Media.Imaging.BitmapImage(uri);
                ImgMainBackground.Opacity = currentUser.Role == "Admin" ? 0.22 : 0.12;
            }
            catch { }

            // Setup welcome text and role badge
            TxtUserRoleBadge.Text = currentUser.Role.ToUpper();
            TxtUserWelcome.Text = $"Welcome back, {currentUser.FullName}!";

            if (currentUser.Role == "Admin")
            {
                TxtUserRoleBadge.Background = (System.Windows.Media.Brush)FindResource("AccentPurpleBrush");
            }
            else
            {
                TxtUserRoleBadge.Background = (System.Windows.Media.Brush)FindResource("AccentTealBrush");
                TxtUserRoleBadge.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(13, 17, 23)); // Dark text for teal

                // Hide customers menu item for Customers
                var items = (HamburgerMenuItemCollection)NavigationMenu.ItemsSource;
                var customersItem = items.OfType<HamburgerMenuGlyphItem>().FirstOrDefault(i => i.Label == "Customers");
                if (customersItem != null)
                {
                    items.Remove(customersItem);
                }
            }

            // Load default view (Dashboard)
            NavigationMenu.SelectedIndex = 0;
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void BtnMaximize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void NavigationMenu_ItemClick(object sender, ItemClickEventArgs args)
        {
            if (args.ClickedItem is HamburgerMenuGlyphItem item)
            {
                NavigateTo(item.Label);
            }
        }

        private void NavigationMenu_OptionsItemClick(object sender, ItemClickEventArgs args)
        {
            if (args.ClickedItem is HamburgerMenuGlyphItem item)
            {
                if (item.Label == "Logout")
                {
                    // Show confirmation dialog
                    var confirmDialog = new ConfirmLogoutWindow();
                    confirmDialog.Owner = this;
                    if (confirmDialog.ShowDialog() == true)
                    {
                        // Clear session and return to Login
                        SessionManager.Logout();
                        var loginWindow = new LoginWindow();
                        loginWindow.Show();
                        this.Close();
                    }
                    else
                    {
                        // Deselect options item
                        NavigationMenu.SelectedOptionsIndex = -1;
                    }
                }
                else
                {
                    NavigateTo(item.Label);
                }
            }
        }

        public void NavigateTo(string label)
        {
            switch (label)
            {
                case "Dashboard":
                    CurrentView = new Dashboard();
                    break;
                case "Properties":
                    CurrentView = new PropertiesView();
                    break;
                case "Customers":
                    CurrentView = new CustomersView();
                    break;
                case "Transactions":
                    CurrentView = new TransactionsView();
                    break;
                case "Settings":
                    CurrentView = new Settings();
                    break;
            }
        }

        private bool isSidebarExpanded = true;

        private void SidebarToggle_Click(object sender, RoutedEventArgs e)
        {
            if (isSidebarExpanded)
            {
                SidebarColumn.Width = new GridLength(60);
                AppNameText.Visibility = Visibility.Collapsed;
                NavigationMenu.IsPaneOpen = false;
                isSidebarExpanded = false;
            }
            else
            {
                SidebarColumn.Width = new GridLength(220);
                AppNameText.Visibility = Visibility.Visible;
                NavigationMenu.IsPaneOpen = true;
                isSidebarExpanded = true;
            }
        }
    }
}
