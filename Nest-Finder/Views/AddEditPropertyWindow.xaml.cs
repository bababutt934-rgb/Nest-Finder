using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using NestFinder.Helpers;
using NestFinder.Models;
using NestFinder.Services;

namespace NestFinder.Views
{
    public partial class AddEditPropertyWindow : MahApps.Metro.Controls.MetroWindow
    {
        private readonly Property? _existing;
        private readonly PropertyRepository _repo = new();
        private readonly ImageRepository _imgRepo = new();
        private readonly ObservableCollection<PropertyImageItem> _imageItems = new();

        public AddEditPropertyWindow(Property? existing)
        {
            InitializeComponent();
            _existing = existing;
            ImagesListBox.ItemsSource = _imageItems;

            if (_existing != null)
            {
                this.Title = "Edit Property";
                DialogTitle.Text = "Edit Property";
                PopulateFields();
            }
            else
            {
                this.Title = "Add Property";
                DialogTitle.Text = "Add New Property";
                UpdateNoImagesPlaceholder();
            }
        }

        private void PopulateFields()
        {
            if (_existing == null) return;

            TitleBox.Text = _existing.Title;
            CityBox.Text = _existing.City;
            PriceBox.Text = _existing.Price.ToString("0");
            AreaBox.Text = _existing.AreaSqFt.ToString();
            
            // Set combo box items
            SetComboBoxValue(TypeBox, _existing.Type);
            SetComboBoxValue(StatusBox, _existing.Status);

            BedroomsBox.Text = _existing.Bedrooms.ToString();
            BathroomsBox.Text = _existing.Bathrooms.ToString();
            DescriptionBox.Text = _existing.Description;

            // Load existing images if any
            try
            {
                var images = _imgRepo.GetByProperty(_existing.Id);
                foreach (var img in images)
                {
                    if (File.Exists(img.ImagePath))
                    {
                        var bmp = new BitmapImage();
                        bmp.BeginInit();
                        bmp.UriSource = new Uri(img.ImagePath, UriKind.RelativeOrAbsolute);
                        bmp.CacheOption = BitmapCacheOption.OnLoad; // releases file lock
                        bmp.EndInit();

                        _imageItems.Add(new PropertyImageItem
                        {
                            Id = img.Id.ToString(),
                            FilePath = img.ImagePath,
                            ImageSource = bmp
                        });
                    }
                }
            }
            catch { }
            UpdateNoImagesPlaceholder();
        }

        private void SetComboBoxValue(System.Windows.Controls.ComboBox comboBox, string value)
        {
            foreach (System.Windows.Controls.ComboBoxItem item in comboBox.Items)
            {
                if (item.Content.ToString() == value)
                {
                    comboBox.SelectedItem = item;
                    break;
                }
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        // Block non-numeric input for Bedrooms and Bathrooms fields
        private void NumberOnly_PreviewTextInput(object sender, 
            System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !int.TryParse(e.Text, out _);
        }

        private void AddPictures_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.jpg, *.jpeg, *.png, *.gif)|*.jpg;*.jpeg;*.png;*.gif",
                Title = "Select Property Images",
                Multiselect = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                foreach (string filePath in openFileDialog.FileNames)
                {
                    if (_imageItems.Any(item => item.FilePath.Equals(filePath, StringComparison.OrdinalIgnoreCase)))
                        continue;

                    try
                    {
                        var bmp = new BitmapImage();
                        bmp.BeginInit();
                        bmp.UriSource = new Uri(filePath);
                        bmp.CacheOption = BitmapCacheOption.OnLoad;
                        bmp.EndInit();

                        _imageItems.Add(new PropertyImageItem
                        {
                            Id = null,
                            FilePath = filePath,
                            ImageSource = bmp
                        });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error loading preview for {Path.GetFileName(filePath)}: {ex.Message}", "Preview Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                UpdateNoImagesPlaceholder();
            }
        }

        private void RemovePicture_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is PropertyImageItem item)
            {
                _imageItems.Remove(item);
                UpdateNoImagesPlaceholder();
            }
        }

        private void UpdateNoImagesPlaceholder()
        {
            TxtNoImages.Visibility = _imageItems.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void SaveProperty_Click(object sender, RoutedEventArgs e)
        {
            string title = TitleBox.Text.Trim();
            string city = CityBox.Text.Trim();

            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(city))
            {
                MessageBox.Show("Title and City are required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(PriceBox.Text, out decimal price) || price < 0)
            {
                MessageBox.Show("Please enter a valid price.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int.TryParse(AreaBox.Text, out int area);
            int.TryParse(BedroomsBox.Text, out int bedrooms);
            int.TryParse(BathroomsBox.Text, out int bathrooms);

            var prop = _existing ?? new Property();
            prop.Title = title;
            prop.City = city;
            prop.Price = price;
            prop.AreaSqFt = area;
            prop.Type = (TypeBox.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content?.ToString() ?? "Residential";
            prop.Status = (StatusBox.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content?.ToString() ?? "Available";
            prop.Bedrooms = bedrooms;
            prop.Bathrooms = bathrooms;
            prop.Description = DescriptionBox.Text.Trim();
            prop.UserId = SessionManager.CurrentUser?.Id;

            bool success;
            int targetId = 0;
            if (_existing != null)
            {
                success = _repo.Update(prop);
                targetId = prop.Id;
            }
            else
            {
                targetId = _repo.Add(prop);
                success = targetId > 0;
            }

            if (success)
            {
                // Handle multiple images selection and persistence
                try
                {
                    var dbImages = _imgRepo.GetByProperty(targetId);

                    // Delete DB image rows that are no longer in _imageItems list
                    foreach (var dbImg in dbImages)
                    {
                        if (!_imageItems.Any(item => item.Id == dbImg.Id.ToString()))
                        {
                            _imgRepo.Delete(dbImg.Id);
                            // Delete copied file from uploads directory
                            if (File.Exists(dbImg.ImagePath) && dbImg.ImagePath.Contains("uploads"))
                            {
                                try { File.Delete(dbImg.ImagePath); } catch { }
                            }
                        }
                    }

                    // Save new images
                    string uploadsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "uploads");
                    if (!Directory.Exists(uploadsDir))
                    {
                        Directory.CreateDirectory(uploadsDir);
                    }

                    foreach (var item in _imageItems)
                    {
                        if (item.Id == null && File.Exists(item.FilePath))
                        {
                            string ext = Path.GetExtension(item.FilePath);
                            string uniqueName = $"prop_{targetId}_{Guid.NewGuid()}{ext}";
                            string destPath = Path.Combine(uploadsDir, uniqueName);

                            File.Copy(item.FilePath, destPath, true);
                            _imgRepo.Add(targetId, destPath);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Property saved, but failed to save images: {ex.Message}", "Images Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                this.DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("An error occurred while saving the property.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public class PropertyImageItem
    {
        public string? Id { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string FileName => Path.GetFileName(FilePath);
        public BitmapImage ImageSource { get; set; } = new();
    }
}
