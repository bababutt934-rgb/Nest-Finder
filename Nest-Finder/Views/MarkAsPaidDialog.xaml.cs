using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using NestFinder.Models;
using NestFinder.Services;

namespace NestFinder.Views
{
    public partial class MarkAsPaidDialog : MahApps.Metro.Controls.MetroWindow
    {
        private readonly TransactionRepository _repo = new();
        private string _selectedMethod = "Cash"; // default selection
        private int    _transactionID;

        public MarkAsPaidDialog()
        {
            InitializeComponent();
        }

        public void LoadTransaction(Transaction tx)
        {
            _transactionID = tx.Id;
            TxSummaryText.Text = $"Tx #{tx.Id}  —  {tx.PropertyTitle}  —  {tx.CustomerName}";
            TxAmountText.Text = tx.AmountFormatted;
        }

        // Called when Cash border is clicked
        private void SelectCash(object sender, MouseButtonEventArgs e)
        {
            _selectedMethod = "Cash";
            HighlightSelected(CashBorder, OnlineBorder);
        }

        // Called when Online border is clicked  
        private void SelectOnline(object sender, MouseButtonEventArgs e)
        {
            _selectedMethod = "Online";
            HighlightSelected(OnlineBorder, CashBorder);
        }

        // Helper: highlight selected, dim unselected
        private void HighlightSelected(Border selected, Border unselected)
        {
            // Selected style
            selected.BorderBrush = new SolidColorBrush(Color.FromRgb(0, 255, 200));
            selected.Background  = new SolidColorBrush(Color.FromArgb(30, 0, 255, 200));

            // Unselected style  
            unselected.BorderBrush = new SolidColorBrush(Color.FromRgb(45, 55, 72));
            unselected.Background  = Brushes.Transparent;

            // Highlight/dim text and icons to match border
            var activeBrush = new SolidColorBrush(Color.FromRgb(0, 255, 200));
            var inactiveBrush = new SolidColorBrush(Color.FromRgb(139, 148, 158));

            if (selected == CashBorder)
            {
                CashIcon.Foreground = activeBrush;
                CashText.Foreground = activeBrush;
                OnlineIcon.Foreground = inactiveBrush;
                OnlineText.Foreground = inactiveBrush;
            }
            else
            {
                OnlineIcon.Foreground = activeBrush;
                OnlineText.Foreground = activeBrush;
                CashIcon.Foreground = inactiveBrush;
                CashText.Foreground = inactiveBrush;
            }
        }

        // Confirm button click — saves to DB
        private void ConfirmPayment_Click(object sender, RoutedEventArgs e)
        {
            bool success = _repo.MarkAsPaid(
                _transactionID, 
                _selectedMethod);

            if (success)
            {
                DialogResult = true;  // ← triggers grid refresh
                Close();
            }
            else
            {
                MessageBox.Show(
                    "Failed to update payment status. Try again.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
