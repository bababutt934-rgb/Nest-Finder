using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace NestFinder.Helpers
{
    // Foreground + Border color converter
    public class PaymentStatusBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, 
            object parameter, CultureInfo culture)
        {
            bool isPaid = string.Equals(value?.ToString()?.Trim(), "Paid", StringComparison.OrdinalIgnoreCase);
            return isPaid
                ? new SolidColorBrush(Color.FromRgb(0, 255, 200))
                : new SolidColorBrush(Color.FromRgb(255, 76, 76));
        }
        public object ConvertBack(object value, Type targetType, 
            object parameter, CultureInfo culture) 
            => throw new NotImplementedException();
    }

    // Background color converter  
    public class PaymentStatusBgConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            bool isPaid = string.Equals(value?.ToString()?.Trim(), "Paid", StringComparison.OrdinalIgnoreCase);
            return isPaid
                ? new SolidColorBrush(Color.FromArgb(30, 0, 255, 200))
                : new SolidColorBrush(Color.FromArgb(30, 255, 76, 76));
        }
        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    // Hide "Mark as Paid" button when already Paid
    public class UnpaidToVisibleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            bool isUnpaid = string.Equals(value?.ToString()?.Trim(), "Unpaid", StringComparison.OrdinalIgnoreCase);
            return isUnpaid
                ? Visibility.Visible
                : Visibility.Collapsed;
        }
        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    // Hides method text when PaymentMethod is null or empty
    public class NullToCollapsedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return string.IsNullOrWhiteSpace(value?.ToString())
                ? Visibility.Collapsed
                : Visibility.Visible;
        }
        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
