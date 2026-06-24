using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using NestFinder.Helpers;
using NestFinder.Models;
using NestFinder.Services;

namespace NestFinder.Views
{
    public partial class LeaveReviewWindow : MahApps.Metro.Controls.MetroWindow
    {
        private readonly int _propertyId;
        private int _rating = 0;
        private readonly ReviewRepository _reviewRepo = new();

        public LeaveReviewWindow(int propertyId)
        {
            InitializeComponent();
            _propertyId = propertyId;
        }

        private void Star_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is FontAwesome.Sharp.IconBlock clickedStar && int.TryParse(clickedStar.Tag?.ToString(), out int rating))
            {
                _rating = rating;
                UpdateStarsUI();
            }
        }

        private void UpdateStarsUI()
        {
            var activeBrush = (Brush)FindResource("AccentAmberBrush");
            var inactiveBrush = (Brush)FindResource("TextMutedBrush");

            // Loop through children in StarsPanel
            for (int i = 0; i < StarsPanel.Children.Count; i++)
            {
                if (StarsPanel.Children[i] is FontAwesome.Sharp.IconBlock star)
                {
                    if (int.TryParse(star.Tag?.ToString(), out int idx))
                    {
                        star.Foreground = idx <= _rating ? activeBrush : inactiveBrush;
                    }
                }
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void BtnSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (_rating == 0)
            {
                MessageBox.Show("Please select a rating by clicking the stars.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var review = new Review
            {
                PropertyId = _propertyId,
                UserId = SessionManager.CurrentUser!.Id,
                Rating = _rating,
                Comment = TxtComment.Text.Trim()
            };

            bool success = _reviewRepo.Add(review);

            if (success)
            {
                MessageBox.Show("Review submitted successfully! Thank you for your feedback.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("Failed to submit review. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
