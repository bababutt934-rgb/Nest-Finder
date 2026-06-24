using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using NestFinder.Helpers;
using NestFinder.Models;
using NestFinder.Services;

namespace NestFinder.Views
{
    public partial class PropertyDetailWindow : MahApps.Metro.Controls.MetroWindow
    {
        private readonly Property _property;
        private readonly ReviewRepository _reviewRepo = new();
        private readonly FavoriteRepository _favRepo = new();
        private readonly TransactionRepository _txRepo = new();
        private readonly ImageRepository _imgRepo = new();
        private bool _isFavorite;
        private System.Collections.Generic.List<PropertyImage> _propertyImages = new();
        private int _currentImageIndex = 0;

        public PropertyDetailWindow(Property property)
        {
            InitializeComponent();
            _property = property;
            LoadPropertyDetails();
        }

        private void LoadPropertyDetails()
        {
            TitleText.Text       = _property.Title;
            PriceText.Text       = $"${_property.Price:N0}";
            CityText.Text        = _property.City;
            TypeText.Text        = _property.Type;
            BedsText.Text        = _property.Bedrooms > 0 
                                   ? _property.Bedrooms.ToString() : "N/A";
            BathsText.Text       = _property.Bathrooms > 0 
                                   ? _property.Bathrooms.ToString() : "N/A";
            AreaText.Text        = _property.AreaSqFt > 0 
                                   ? $"{_property.AreaSqFt:N0} sqft" : "N/A";
            ListedDateText.Text  = _property.CreatedAt.ToString("MMM dd, yyyy");
            DescriptionText.Text = string.IsNullOrEmpty(_property.Description) 
                                   ? "No description provided." 
                                   : _property.Description;

            // Status badge overlay color
            StatusOverlayText.Text = _property.Status;
            StatusOverlay.Background = _property.Status switch {
                "Available" => new SolidColorBrush(Color.FromRgb(0, 255, 200)) { Opacity = 0.15 },
                "Sold"      => new SolidColorBrush(Color.FromRgb(255, 76, 76))  { Opacity = 0.15 },
                "Rented"    => new SolidColorBrush(Color.FromRgb(240, 165, 0))  { Opacity = 0.15 },
                _           => Brushes.Transparent
            };
            StatusOverlayText.Foreground = _property.Status switch {
                "Available" => new SolidColorBrush(Color.FromRgb(0, 255, 200)),
                "Sold"      => new SolidColorBrush(Color.FromRgb(255, 76, 76)),
                "Rented"    => new SolidColorBrush(Color.FromRgb(240, 165, 0)),
                _           => Brushes.White
            };

            // Buy/Rent buttons: only show if Available and user is Customer
            bool isCustomer = SessionManager.CurrentUser?.Role == "Customer";
            bool canTransact = _property.Status == "Available" && isCustomer;
            BuyBtn.Visibility  = canTransact ? Visibility.Visible : Visibility.Collapsed;
            RentBtn.Visibility = canTransact ? Visibility.Visible : Visibility.Collapsed;

            // Status message for sold/rented
            StatusMessageText.Text = _property.Status switch {
                "Sold"   => "This property has been sold.",
                "Rented" => "This property is currently rented.",
                _        => string.Empty
            };

            // Load images using ImageRepository
            _propertyImages = _imgRepo.GetByProperty(_property.Id);
            if (_propertyImages.Count > 0)
            {
                _currentImageIndex = 0;
                ShowImage(_currentImageIndex);
                
                // Show/hide navigation grid
                if (_propertyImages.Count > 1)
                {
                    ImageNavigationGrid.Visibility = Visibility.Visible;
                }
                else
                {
                    ImageNavigationGrid.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                MainPropertyImage.Source = null;
                ImagePlaceholder.Visibility = Visibility.Visible;
                ImageNavigationGrid.Visibility = Visibility.Collapsed;
            }

            // Rating & Reviews
            LoadReviews();

            // Show Leave Review button only if customer has a completed transaction for this property
            if (isCustomer)
            {
                bool hasTransaction = _txRepo.CustomerHasTransaction(SessionManager.CurrentUser!.Id, _property.Id);
                LeaveReviewBtn.Visibility = hasTransaction ? Visibility.Visible : Visibility.Collapsed;

                // Favorites: check if already saved
                _isFavorite = _favRepo.IsFavorite(SessionManager.CurrentUser.Id, _property.Id);
                UpdateFavoriteButtonText();
            }
            else
            {
                LeaveReviewBtn.Visibility = Visibility.Collapsed;
                FavoriteBtn.Visibility = Visibility.Collapsed; // Hide favorite actions for admin
            }

            // Start loading external weather and exchange rate details
            _ = LoadExternalApiData();
        }

        private async System.Threading.Tasks.Task LoadExternalApiData()
        {
            var apiService = new ExternalApiService();

            // 1. Fetch USD to PKR Exchange Rate & convert property price
            try
            {
                var rate = await apiService.GetUsdToPkrExchangeRateAsync();
                if (rate.HasValue)
                {
                    decimal pkrPrice = _property.Price * rate.Value;
                    PkrPriceText.Text = $"PKR {pkrPrice:N0}";
                    ExchangeRateText.Text = $"1 USD = {rate.Value:F2} PKR";
                    
                    ExchangeLoadingPanel.Visibility = Visibility.Collapsed;
                    ExchangeErrorPanel.Visibility = Visibility.Collapsed;
                    ExchangeInfoPanel.Visibility = Visibility.Visible;
                }
                else
                {
                    ExchangeLoadingPanel.Visibility = Visibility.Collapsed;
                    ExchangeErrorPanel.Visibility = Visibility.Visible;
                }
            }
            catch
            {
                ExchangeLoadingPanel.Visibility = Visibility.Collapsed;
                ExchangeErrorPanel.Visibility = Visibility.Visible;
            }

            // 2. Fetch Weather for City
            if (!string.IsNullOrWhiteSpace(_property.City))
            {
                try
                {
                    var weather = await apiService.GetWeatherForCityAsync(_property.City);
                    if (weather != null)
                    {
                        WeatherTempText.Text = $"{weather.Temperature:F1}°C";
                        WeatherDescText.Text = weather.Description;
                        WeatherDetailText.Text = $"Feels like: {weather.ApparentTemperature:F1}°C • Humidity: {weather.Humidity:F0}%";
                        
                        if (Enum.TryParse<FontAwesome.Sharp.IconChar>(weather.Icon, out var iconChar))
                        {
                            WeatherIcon.Icon = iconChar;
                        }
                        else
                        {
                            WeatherIcon.Icon = FontAwesome.Sharp.IconChar.CloudSun;
                        }

                        WeatherLoadingPanel.Visibility = Visibility.Collapsed;
                        WeatherErrorPanel.Visibility = Visibility.Collapsed;
                        WeatherInfoPanel.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        WeatherLoadingPanel.Visibility = Visibility.Collapsed;
                        WeatherErrorPanel.Visibility = Visibility.Visible;
                    }
                }
                catch
                {
                    WeatherLoadingPanel.Visibility = Visibility.Collapsed;
                    WeatherErrorPanel.Visibility = Visibility.Visible;
                }
            }
            else
            {
                WeatherLoadingPanel.Visibility = Visibility.Collapsed;
                WeatherErrorPanel.Visibility = Visibility.Visible;
            }
        }

        private void ShowImage(int index)
        {
            if (index < 0 || index >= _propertyImages.Count) return;

            var img = _propertyImages[index];
            if (File.Exists(img.ImagePath))
            {
                try
                {
                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.UriSource = new Uri(img.ImagePath, UriKind.RelativeOrAbsolute);
                    bmp.CacheOption = BitmapCacheOption.OnLoad; // releases file lock
                    bmp.EndInit();

                    MainPropertyImage.Source = bmp;
                    ImagePlaceholder.Visibility = Visibility.Collapsed;
                    TxtImageIndex.Text = $"{index + 1} / {_propertyImages.Count}";
                }
                catch
                {
                    MainPropertyImage.Source = null;
                    ImagePlaceholder.Visibility = Visibility.Visible;
                }
            }
            else
            {
                MainPropertyImage.Source = null;
                ImagePlaceholder.Visibility = Visibility.Visible;
            }
        }

        private void BtnPrevImage_Click(object sender, RoutedEventArgs e)
        {
            if (_propertyImages.Count <= 1) return;
            _currentImageIndex--;
            if (_currentImageIndex < 0)
            {
                _currentImageIndex = _propertyImages.Count - 1; // wrap to end
            }
            ShowImage(_currentImageIndex);
        }

        private void BtnNextImage_Click(object sender, RoutedEventArgs e)
        {
            if (_propertyImages.Count <= 1) return;
            _currentImageIndex++;
            if (_currentImageIndex >= _propertyImages.Count)
            {
                _currentImageIndex = 0; // wrap to start
            }
            ShowImage(_currentImageIndex);
        }

        private void LoadReviews()
        {
            var reviews = _reviewRepo.GetByProperty(_property.Id);
            ReviewCountText.Text = reviews.Count > 0 ? $"({reviews.Count})" : "(0)";

            // Average rating
            double avg = reviews.Count > 0 ? reviews.Average(r => r.Rating) : 0;
            RatingText.Text = avg > 0 ? avg.ToString("0.0") : "No ratings";

            if (reviews.Count == 0)
            {
                NoReviewsPanel.Visibility = Visibility.Visible;
                ReviewsList.Visibility    = Visibility.Collapsed;
            }
            else
            {
                NoReviewsPanel.Visibility = Visibility.Collapsed;
                ReviewsList.Visibility    = Visibility.Visible;
                
                ReviewsList.ItemsSource = reviews.Select(r => new
                {
                    InitialsText = GetInitials(r.ReviewerName),
                    ReviewerName = r.ReviewerName,
                    ReviewDateFormatted = r.CreatedAt.ToString("MMM dd, yyyy"),
                    ReviewText = r.Comment,
                    StarColors = Enumerable.Range(1, 5).Select(i => i <= r.Rating ? "#F0A500" : "#2D3748").ToList()
                }).ToList();
            }
        }

        private static string GetInitials(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "U";
            var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1) return parts[0][0].ToString().ToUpper();
            return (parts[0][0].ToString() + parts[parts.Length - 1][0].ToString()).ToUpper();
        }

        private void UpdateFavoriteButtonText()
        {
            if (_isFavorite)
            {
                FavoriteBtnText.Text = "Saved ♥";
                FavoriteIcon.Foreground = new SolidColorBrush(Color.FromRgb(255, 76, 76));
            }
            else
            {
                FavoriteBtnText.Text = "Add Favorite";
                FavoriteIcon.Foreground = new SolidColorBrush(Color.FromRgb(0, 255, 200));
            }
        }

        private void ToggleFavorite_Click(object sender, RoutedEventArgs e)
        {
            if (SessionManager.CurrentUser == null) return;
            int userId = SessionManager.CurrentUser.Id;
            if (_isFavorite)
            {
                _favRepo.Remove(userId, _property.Id);
                _isFavorite = false;
            }
            else
            {
                _favRepo.Add(userId, _property.Id);
                _isFavorite = true;
            }
            UpdateFavoriteButtonText();
        }

        private void LeaveReview_Click(object sender, RoutedEventArgs e)
        {
            var win = new LeaveReviewWindow(_property.Id);
            win.Owner = this;
            win.ShowDialog();
            LoadReviews(); // Reload after user writes review
        }

        private void BuyProperty_Click(object sender, RoutedEventArgs e)
        {
            var win = new BuyRentWindow(_property, "Buy");
            win.Owner = this;
            if (win.ShowDialog() == true)
            {
                this.DialogResult = true; // refresh parent properties page
                this.Close();
            }
        }

        private void RentProperty_Click(object sender, RoutedEventArgs e)
        {
            var win = new BuyRentWindow(_property, "Rent");
            win.Owner = this;
            if (win.ShowDialog() == true)
            {
                this.DialogResult = true; // refresh parent properties page
                this.Close();
            }
        }
    }
}
