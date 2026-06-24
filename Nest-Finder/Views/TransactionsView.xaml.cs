using System.Windows;
using System.Windows.Controls;
using NestFinder.Helpers;
using NestFinder.Services;
using NestFinder.Models;
using Microsoft.Win32;

namespace NestFinder.Views
{
    public partial class TransactionsView : UserControl
    {
        private readonly TransactionRepository _repo = new();

        public TransactionsView()
        {
            InitializeComponent();
            Loaded += TransactionsView_Loaded;
        }

        private void TransactionsView_Loaded(object sender, RoutedEventArgs e)
        {
            bool isAdmin = SessionManager.IsAdmin;
            
            if (isAdmin)
            {
                TxtTitle.Text = "ALL TRANSACTIONS";
                BtnExport.Visibility = Visibility.Visible;
                ColCustomer.Visibility = Visibility.Visible;
                ColAction.Visibility = Visibility.Visible;
            }
            else
            {
                TxtTitle.Text = "MY TRANSACTIONS";
                BtnExport.Visibility = Visibility.Collapsed;
                ColCustomer.Visibility = Visibility.Collapsed;
                ColAction.Visibility = Visibility.Collapsed;
            }

            // Default dates
            DtpFrom.SelectedDate = System.DateTime.Now.AddMonths(-6);
            DtpTo.SelectedDate = System.DateTime.Now;

            LoadData();
        }

        private void LoadTransactions()
        {
            LoadData();
        }

        private void LoadData()
        {
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

            System.DateTime from = DtpFrom.SelectedDate ?? System.DateTime.Now.AddMonths(-6);
            System.DateTime to = DtpTo.SelectedDate ?? System.DateTime.Now;
            
            // Adjust to cover end of day
            to = to.Date.AddDays(1).AddSeconds(-1);

            if (SessionManager.IsAdmin)
            {
                TransactionsGrid.ItemsSource = _repo.GetAll(type, status, from, to);
            }
            else
            {
                // Customer sees only their own
                var allCustTrans = _repo.GetByCustomerId(SessionManager.CurrentUser!.Id);
                var filtered = new System.Collections.Generic.List<Transaction>();
                
                foreach (var t in allCustTrans)
                {
                    if (type != null && t.TransactionType != type) continue;
                    if (status != null && t.Status != status) continue;
                    if (t.Date < from || t.Date > to) continue;
                    filtered.Add(t);
                }

                TransactionsGrid.ItemsSource = filtered;
            }
        }

        private void BtnFilter_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            var sfd = new SaveFileDialog { Filter = "CSV Files (*.csv)|*.csv", FileName = "transactions.csv" };
            if (sfd.ShowDialog() == true)
            {
                var currentItems = TransactionsGrid.ItemsSource as System.Collections.Generic.IEnumerable<Transaction>;
                if (currentItems != null)
                {
                    var list = new System.Collections.Generic.List<Transaction>(currentItems);
                    _repo.ExportToCsv(list, sfd.FileName);
                    MessageBox.Show("Transactions exported successfully!", "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void MarkAsPaid_Click(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            var tx  = (Transaction)btn.DataContext;

            var dialog       = new MarkAsPaidDialog();
            dialog.Owner     = Window.GetWindow(this);
            dialog.LoadTransaction(tx);

            if (dialog.ShowDialog() == true)
            {
                LoadTransactions(); // ← refreshes grid, 
                                    //   shows Paid badge
            }
        }
    }
}
