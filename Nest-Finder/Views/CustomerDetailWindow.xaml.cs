using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using NestFinder.Helpers;
using NestFinder.Services;

namespace NestFinder.Views
{
    public partial class CustomerDetailWindow : MahApps.Metro.Controls.MetroWindow
    {
        private readonly int _userId;
        private readonly UserRepository _userRepo = new();
        private readonly TransactionRepository _txRepo = new();
        private bool _wasModified = false;

        public CustomerDetailWindow(int userId)
        {
            InitializeComponent();
            _userId = userId;
            LoadCustomerDetails();
        }

        private void LoadCustomerDetails()
        {
            var user = _userRepo.GetById(_userId);
            if (user == null)
            {
                this.Close();
                return;
            }

            // Initials
            if (!string.IsNullOrWhiteSpace(user.FullName))
            {
                var parts = user.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                AvatarInitials.Text = parts.Length >= 2
                    ? $"{parts[0][0]}{parts[parts.Length - 1][0]}".ToUpper()
                    : user.FullName.Substring(0, Math.Min(2, user.FullName.Length)).ToUpper();
            }
            else
            {
                AvatarInitials.Text = "C";
            }

            CustomerNameText.Text = user.FullName;
            EmailText.Text        = user.Email;
            PhoneText.Text        = string.IsNullOrEmpty(user.Phone) ? "Not provided" : user.Phone;
            MemberSinceText.Text  = user.CreatedAt.ToString("MMM yyyy");

            // Status badge styling
            bool isActive = user.IsActive;
            StatusBadgeText.Text = isActive ? "Active" : "Inactive";
            StatusBadgeBorder.Background = isActive
                ? new SolidColorBrush(Color.FromArgb(30, 0, 255, 200))
                : new SolidColorBrush(Color.FromArgb(30, 255, 76, 76));
            StatusBadgeBorder.BorderBrush = isActive
                ? new SolidColorBrush(Color.FromRgb(0, 255, 200))
                : new SolidColorBrush(Color.FromRgb(255, 76, 76));
            StatusBadgeBorder.BorderThickness = new Thickness(1);
            StatusBadgeText.Foreground = isActive
                ? new SolidColorBrush(Color.FromRgb(0, 255, 200))
                : new SolidColorBrush(Color.FromRgb(255, 76, 76));

            // Status note and Toggle button
            if (user.Id == SessionManager.CurrentUser?.Id)
            {
                ToggleStatusBtn.Visibility = Visibility.Collapsed;
                ActionHintText.Text = "You are viewing your own profile.";
            }
            else
            {
                ToggleStatusBtn.Visibility = Visibility.Visible;
                ToggleStatusBtn.Content = isActive ? "Deactivate Account" : "Activate Account";
                ToggleStatusBtn.Background = isActive
                    ? new SolidColorBrush(Color.FromRgb(255, 76, 76))
                    : new SolidColorBrush(Color.FromRgb(0, 255, 200));
                ToggleStatusBtn.Foreground = isActive
                    ? Brushes.White
                    : new SolidColorBrush(Color.FromRgb(13, 17, 23));
                ActionHintText.Text = isActive
                    ? "Customer account is currently active."
                    : "This account has been deactivated.";
            }

            // Load transactions
            var transactions = _txRepo.GetByCustomerId(_userId);
            TotalTxText.Text = transactions.Count.ToString();

            if (transactions.Count == 0)
            {
                TransactionsGrid.Visibility = Visibility.Collapsed;
                NoTxPanel.Visibility        = Visibility.Visible;
            }
            else
            {
                TransactionsGrid.Visibility = Visibility.Visible;
                NoTxPanel.Visibility        = Visibility.Collapsed;
                TransactionsGrid.ItemsSource = transactions.Select(t => new
                {
                    PropertyTitle = t.PropertyTitle,
                    TransactionType = t.TransactionType,
                    AmountFormatted = $"${t.Amount:N0}",
                    DateFormatted = t.Date.ToString("yyyy-MM-dd"),
                    Status = t.Status,
                    StatusBadgeBg = t.Status switch
                    {
                        "Completed" => new SolidColorBrush(Color.FromRgb(0, 255, 200)) { Opacity = 0.15 },
                        "Pending" => new SolidColorBrush(Color.FromRgb(240, 165, 0)) { Opacity = 0.15 },
                        "Cancelled" => new SolidColorBrush(Color.FromRgb(255, 76, 76)) { Opacity = 0.15 },
                        _ => Brushes.Transparent
                    },
                    StatusBadgeBorder = t.Status switch
                    {
                        "Completed" => new SolidColorBrush(Color.FromRgb(0, 255, 200)),
                        "Pending" => new SolidColorBrush(Color.FromRgb(240, 165, 0)),
                        "Cancelled" => new SolidColorBrush(Color.FromRgb(255, 76, 76)),
                        _ => Brushes.Transparent
                    },
                    StatusBadgeText = t.Status switch
                    {
                        "Completed" => new SolidColorBrush(Color.FromRgb(0, 255, 200)),
                        "Pending" => new SolidColorBrush(Color.FromRgb(240, 165, 0)),
                        "Cancelled" => new SolidColorBrush(Color.FromRgb(255, 76, 76)),
                        _ => Brushes.White
                    }
                }).ToList();
            }
        }

        private void ToggleStatus_Click(object sender, RoutedEventArgs e)
        {
            if (_userId == SessionManager.CurrentUser?.Id)
            {
                MessageBox.Show("You cannot deactivate your own account.", "Action Prohibited", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            _wasModified = true;
            _userRepo.ToggleActive(_userId);
            LoadCustomerDetails();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            if (_wasModified)
            {
                try
                {
                    this.DialogResult = true;
                }
                catch
                {
                    // Safe guard if not shown via ShowDialog
                }
            }
            this.Close();
        }
    }
}
