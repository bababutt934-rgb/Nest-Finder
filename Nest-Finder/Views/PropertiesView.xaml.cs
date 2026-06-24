using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using NestFinder.Helpers;
using NestFinder.Models;
using NestFinder.Services;

namespace NestFinder.Views
{
    public partial class PropertiesView : UserControl
    {
        private readonly PropertyRepository _repo = new();

        public static readonly DependencyProperty IsAdminRoleProperty =
            DependencyProperty.Register("IsAdminRole", typeof(bool), typeof(PropertiesView), new PropertyMetadata(false));

        public bool IsAdminRole
        {
            get { return (bool)GetValue(IsAdminRoleProperty); }
            set { SetValue(IsAdminRoleProperty, value); }
        }

        public PropertiesView()
        {
            InitializeComponent();
            DataContext = this;
            Loaded += PropertiesView_Loaded;
        }

        private void PropertiesView_Loaded(object sender, RoutedEventArgs e)
        {
            IsAdminRole = SessionManager.IsAdmin;
            if (IsAdminRole)
            {
                TxtTitle.Text = "MANAGE PROPERTIES";
                BtnAddProperty.Visibility = Visibility.Visible;
            }
            else
            {
                TxtTitle.Text = "BROWSE PROPERTIES";
                BtnAddProperty.Visibility = Visibility.Collapsed;
            }

            // Init Search Placeholder
            ResetSearchPlaceholder();

            LoadData();
        }

        private void ResetSearchPlaceholder()
        {
            TxtSearch.Text = "Search by title or city...";
            TxtSearch.Foreground = (Brush)FindResource("TextMutedBrush");
        }

        private void TxtSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TxtSearch.Text == "Search by title or city...")
            {
                TxtSearch.Text = "";
                TxtSearch.Foreground = Brushes.White;
            }
        }

        private void TxtSearch_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtSearch.Text))
            {
                ResetSearchPlaceholder();
            }
        }

        private void LoadData()
        {
            string? search = TxtSearch.Text == "Search by title or city..." ? null : TxtSearch.Text.Trim();
            
            string? type = null;
            if (CmbType.SelectedItem is ComboBoxItem typeItem && typeItem.Content.ToString() != "All Types")
            {
                type = typeItem.Content.ToString();
            }

            string? status = null;
            if (CmbStatus.SelectedItem is ComboBoxItem statusItem && statusItem.Content.ToString() != "All Status")
            {
                status = statusItem.Content.ToString();
            }

            PropertiesGrid.ItemsSource = _repo.GetAll(search, type, status);
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void BtnAddProperty_Click(object sender, RoutedEventArgs e)
        {
            var win = new AddEditPropertyWindow(null);
            win.Owner = Window.GetWindow(this);
            if (win.ShowDialog() == true)
            {
                LoadData();
            }
        }

        private void BtnEditProperty_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Property prop)
            {
                var win = new AddEditPropertyWindow(prop);
                win.Owner = Window.GetWindow(this);
                if (win.ShowDialog() == true)
                {
                    LoadData();
                }
            }
        }

        private void BtnDeleteProperty_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Property prop)
            {
                var confirm = MessageBox.Show($"Are you sure you want to delete '{prop.Title}'?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (confirm == MessageBoxResult.Yes)
                {
                    _repo.Delete(prop.Id);
                    LoadData();
                }
            }
        }

        private void BtnViewDetails_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Property prop)
            {
                var win = new PropertyDetailWindow(prop);
                win.Owner = Window.GetWindow(this);
                win.ShowDialog();
                LoadData(); // reload in case status changed (buy/rent/favorite)
            }
        }
    }
}
