using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using NestFinder.Helpers;
using NestFinder.Models;
using NestFinder.Services;

namespace NestFinder.Views
{
    public partial class LoginWindow : MahApps.Metro.Controls.MetroWindow
    {
        private readonly UserRepository _userRepository = new();

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void ToggleForm_Click(object sender, MouseButtonEventArgs e)
        {
            if (LoginGrid.Visibility == Visibility.Visible)
            {
                LoginGrid.Visibility = Visibility.Collapsed;
                RegisterGrid.Visibility = Visibility.Visible;
                LoginErrorBorder.Visibility = Visibility.Collapsed;
            }
            else
            {
                RegisterGrid.Visibility = Visibility.Collapsed;
                LoginGrid.Visibility = Visibility.Visible;
                RegisterStatusBorder.Visibility = Visibility.Collapsed;
            }
        }

        private void TxtLogin_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BtnLogin_Click(sender, e);
            }
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = TxtLoginUsername.Text.Trim();
            string password = TxtLoginPassword.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                TxtLoginError.Text = "Please enter both username and password.";
                LoginErrorBorder.Visibility = Visibility.Visible;
                return;
            }

            string passwordHash = HashHelper.ComputeSha256Hash(password);
            User? authenticatedUser = _userRepository.Authenticate(username, passwordHash);

            if (authenticatedUser != null)
            {
                SessionManager.CurrentUser = authenticatedUser;

                // Open MainWindow
                var mainWindow = new MainWindow();
                Application.Current.MainWindow = mainWindow;
                mainWindow.Show();

                // Close LoginWindow
                this.Close();
            }
            else
            {
                TxtLoginError.Text = "Invalid username, password, or your account is deactivated.";
                LoginErrorBorder.Visibility = Visibility.Visible;
            }
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            string fullName = TxtRegFullName.Text.Trim();
            string phone = TxtRegPhone.Text.Trim();
            string email = TxtRegEmail.Text.Trim();
            string username = TxtRegUsername.Text.Trim();
            string password = TxtRegPassword.Password;
            string confirmPassword = TxtRegConfirmPassword.Password;
            string role = (CmbRegRole.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content?.ToString() ?? "Customer";

            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(phone) ||
                string.IsNullOrEmpty(email) || string.IsNullOrEmpty(username) ||
                string.IsNullOrEmpty(password))
            {
                ShowRegisterStatus("All fields are required.", true);
                return;
            }

            if (password != confirmPassword)
            {
                ShowRegisterStatus("Passwords do not match.", true);
                return;
            }

            // Simple validation rules
            if (password.Length < 6)
            {
                ShowRegisterStatus("Password must be at least 6 characters.", true);
                return;
            }

            string passwordHash = HashHelper.ComputeSha256Hash(password);
            User newUser = role == "Admin" ? new Admin() : new Customer();
            newUser.FullName = fullName;
            newUser.Phone = phone;
            newUser.Email = email;
            newUser.Username = username;
            newUser.PasswordHash = passwordHash;
            newUser.Role = role;

            bool success = _userRepository.Register(newUser);

            if (success)
            {
                // Clear fields
                TxtRegFullName.Clear();
                TxtRegPhone.Clear();
                TxtRegEmail.Clear();
                TxtRegUsername.Clear();
                TxtRegPassword.Clear();
                TxtRegConfirmPassword.Clear();

                ShowRegisterStatus("Account registered successfully! You can now log in.", false);
                
                // Show login card
                RegisterGrid.Visibility = Visibility.Collapsed;
                LoginGrid.Visibility = Visibility.Visible;
                
                // Prefill username
                TxtLoginUsername.Text = username;
                TxtLoginPassword.Focus();
            }
            else
            {
                ShowRegisterStatus("Username or Email already exists.", true);
            }
        }

        private void ShowRegisterStatus(string message, bool isError)
        {
            TxtRegisterStatus.Text = message;
            TxtRegisterStatus.Foreground = isError ? (SolidColorBrush)FindResource("DangerRedBrush") : (SolidColorBrush)FindResource("AccentTealBrush");
            RegisterStatusBorder.Background = isError ? new SolidColorBrush(System.Windows.Media.Color.FromArgb(32, 255, 76, 76)) : new SolidColorBrush(System.Windows.Media.Color.FromArgb(32, 0, 255, 200));
            RegisterStatusBorder.BorderBrush = isError ? (SolidColorBrush)FindResource("DangerRedBrush") : (SolidColorBrush)FindResource("AccentTealBrush");
            RegisterStatusBorder.Visibility = Visibility.Visible;
        }
    }
}
