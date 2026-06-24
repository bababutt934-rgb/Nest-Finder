using System.Windows;

namespace NestFinder.Views
{
    public partial class ConfirmLogoutWindow : Window
    {
        public ConfirmLogoutWindow()
        {
            InitializeComponent();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
