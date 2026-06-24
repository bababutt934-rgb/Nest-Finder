using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using NestFinder.Helpers;
using NestFinder.Models;
using NestFinder.Services;

namespace NestFinder.Views
{
    public partial class Settings : UserControl
    {
        private readonly UserRepository _userRepository = new();

        public Settings()
        {
            InitializeComponent();
            Loaded += Settings_Loaded;
        }

        private void Settings_Loaded(object sender, RoutedEventArgs e)
        {
            LoadUserProfile();
        }


        private void LoadUserProfile()
        {
            var user = SessionManager.CurrentUser;
            if (user == null) return;

            TxtFullName.Text = user.FullName;
            TxtEmail.Text = user.Email;
            TxtPhone.Text = user.Phone;
            TxtRoleBadge.Text = user.Role;

            // Initials logic
            TxtAvatarInitials.Text = GetInitials(user.FullName);
        }

        private string GetInitials(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "SA";
            var parts = name.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1) return parts[0][0].ToString().ToUpper();
            return (parts[0][0].ToString() + parts[parts.Length - 1][0].ToString()).ToUpper();
        }


        private void BtnTogglePassword_Click(object sender, RoutedEventArgs e)
        {
            ExpanderPassword.IsExpanded = !ExpanderPassword.IsExpanded;
        }

        private void BtnSaveProfile_Click(object sender, RoutedEventArgs e)
        {
            var user = SessionManager.CurrentUser;
            if (user == null) return;

            string fullName = TxtFullName.Text.Trim();
            string phone = TxtPhone.Text.Trim();

            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(phone))
            {
                ShowStatus("Full Name and Phone cannot be empty.", true);
                return;
            }

            user.FullName = fullName;
            user.Phone = phone;

            // If user is trying to change password
            if (ExpanderPassword.IsExpanded && !string.IsNullOrEmpty(TxtCurrentPassword.Password))
            {
                string curPasswordHash = HashHelper.ComputeSha256Hash(TxtCurrentPassword.Password);
                if (curPasswordHash != user.PasswordHash)
                {
                    ShowStatus("Current password is incorrect.", true);
                    return;
                }

                if (string.IsNullOrEmpty(TxtNewPassword.Password) || TxtNewPassword.Password.Length < 6)
                {
                    ShowStatus("New password must be at least 6 characters.", true);
                    return;
                }

                if (TxtNewPassword.Password != TxtConfirmNewPassword.Password)
                {
                    ShowStatus("New passwords do not match.", true);
                    return;
                }

                user.PasswordHash = HashHelper.ComputeSha256Hash(TxtNewPassword.Password);
            }

            bool success = _userRepository.UpdateProfile(user);

            if (success)
            {
                // Refresh avatar initials
                TxtAvatarInitials.Text = GetInitials(user.FullName);
                
                // Clear password fields
                TxtCurrentPassword.Clear();
                TxtNewPassword.Clear();
                TxtConfirmNewPassword.Clear();
                ExpanderPassword.IsExpanded = false;

                ShowStatus("Profile updated successfully.", false);

                // Update MainWindow Header Info
                var mainWin = Application.Current.MainWindow as MainWindow;
                if (mainWin != null)
                {
                    mainWin.TxtUserWelcome.Text = $"Welcome back, {user.FullName}!";
                }
            }
            else
            {
                ShowStatus("Failed to update profile. Please try again.", true);
            }
        }

        private void ShowStatus(string message, bool isError)
        {
            TxtProfileAlert.Text = message;
            ProfileAlertBorder.Background = isError ? new SolidColorBrush(Color.FromArgb(32, 255, 76, 76)) : new SolidColorBrush(Color.FromArgb(32, 0, 255, 200));
            ProfileAlertBorder.BorderBrush = isError ? (Brush)FindResource("DangerRedBrush") : (Brush)FindResource("AccentTealBrush");
            ProfileAlertBorder.BorderThickness = new Thickness(1);
            TxtProfileAlert.Foreground = isError ? (Brush)FindResource("DangerRedBrush") : (Brush)FindResource("AccentTealBrush");
            ProfileAlertBorder.Visibility = Visibility.Visible;
        }
    }
}
