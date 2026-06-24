using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using NestFinder.Helpers;
using NestFinder.Models;
using NestFinder.Services;

namespace NestFinder.Views
{
    public partial class CustomersView : UserControl
    {
        private readonly UserRepository _repo = new();

        public CustomersView()
        {
            InitializeComponent();
            Loaded += CustomersView_Loaded;
        }

        private void CustomersView_Loaded(object sender, RoutedEventArgs e)
        {
            ResetSearchPlaceholder();
            LoadData();
        }

        private void ResetSearchPlaceholder()
        {
            TxtSearch.Text = "Search by name or email...";
            TxtSearch.Foreground = (Brush)FindResource("TextMutedBrush");
        }

        private void TxtSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TxtSearch.Text == "Search by name or email...")
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
            string term = TxtSearch.Text == "Search by name or email..." ? "" : TxtSearch.Text.Trim().ToLower();
            var allUsers = _repo.GetAll();
            var filtered = new System.Collections.Generic.List<User>();

            foreach (var u in allUsers)
            {
                if (!string.IsNullOrEmpty(term))
                {
                    if (!u.FullName.ToLower().Contains(term) && !u.Email.ToLower().Contains(term))
                    {
                        continue;
                    }
                }
                filtered.Add(u);
            }

            CustomersGrid.ItemsSource = filtered;
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void BtnViewProfile_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is User user)
            {
                var win = new CustomerDetailWindow(user.Id);
                win.Owner = Window.GetWindow(this);
                win.ShowDialog();
                LoadData(); // Reload in case status changes inside profile
            }
        }

        private void BtnToggleActive_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is User user)
            {
                if (user.Id == SessionManager.CurrentUser?.Id)
                {
                    MessageBox.Show("You cannot deactivate your own account.", "Action Prohibited", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                _repo.ToggleActive(user.Id);
                LoadData();
            }
        }
    }
}
