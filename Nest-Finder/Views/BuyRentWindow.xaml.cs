using System.Windows;
using NestFinder.Helpers;
using NestFinder.Models;
using NestFinder.Services;

namespace NestFinder.Views
{
    public partial class BuyRentWindow : MahApps.Metro.Controls.MetroWindow
    {
        private readonly Property _property;
        private readonly string _type;
        private readonly TransactionRepository _txRepo = new();
        private readonly PropertyRepository _propRepo = new();

        public BuyRentWindow(Property property, string type)
        {
            InitializeComponent();
            _property = property;
            _type = type;

            LoadData();
        }

        private void LoadData()
        {
            TxtProperty.Text = _property.Title;
            TxtType.Text = _type;
            TxtAmount.Text = _property.Price.ToString("C0");
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            var tx = new Transaction
            {
                PropertyId = _property.Id,
                CustomerId = SessionManager.CurrentUser!.Id,
                TransactionType = _type,
                Amount = _property.Price,
                PaymentMethod = (CmbPayment.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content?.ToString() ?? "Cash",
                Status = "Pending"
            };

            bool txSuccess = _txRepo.Add(tx);

            if (txSuccess)
            {
                // Update property status
                _property.Status = _type == "Buy" ? "Sold" : "Rented";
                bool propSuccess = _propRepo.Update(_property);

                if (propSuccess)
                {
                    MessageBox.Show("Transaction completed successfully! Your purchase/rental is pending admin verification.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Failed to update property status, but transaction recorded.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    this.DialogResult = true;
                    this.Close();
                }
            }
            else
            {
                MessageBox.Show("Failed to process transaction. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
