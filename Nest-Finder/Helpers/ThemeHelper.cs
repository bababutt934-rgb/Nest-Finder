using System;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace NestFinder.Helpers
{
    public static class ThemeHelper
    {
        private static readonly string ThemeFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "theme.txt");
        public static string CurrentTheme { get; private set; } = "Dark";

        public static void Initialize()
        {
            LoadTheme();
            ApplyTheme(CurrentTheme);
        }

        public static void LoadTheme()
        {
            try
            {
                if (File.Exists(ThemeFilePath))
                {
                    string saved = File.ReadAllText(ThemeFilePath).Trim();
                    if (saved == "Dark" || saved == "Light" || saved == "System")
                    {
                        CurrentTheme = saved;
                    }
                }
            }
            catch { }
        }

        public static void SaveTheme(string theme)
        {
            try
            {
                File.WriteAllText(ThemeFilePath, theme);
            }
            catch { }
        }

        public static void ApplyTheme(string themeName)
        {
            CurrentTheme = themeName;
            SaveTheme(themeName);

            string activeTheme = themeName;
            if (themeName == "System")
            {
                activeTheme = GetSystemTheme();
            }

            if (activeTheme == "Light")
            {
                // Light mode colors
                Application.Current.Resources["MainBgColor"] = Color.FromRgb(243, 244, 246); // #F3F4F6
                Application.Current.Resources["SidebarBgColor"] = Color.FromRgb(255, 255, 255); // #FFFFFF
                Application.Current.Resources["CardBgColor"] = Color.FromRgb(255, 255, 255); // #FFFFFF
                Application.Current.Resources["TextPrimaryColor"] = Color.FromRgb(17, 24, 39); // #111827
                Application.Current.Resources["TextMutedColor"] = Color.FromRgb(107, 114, 128); // #6B7280
                Application.Current.Resources["BorderColor"] = Color.FromRgb(229, 231, 235); // #E5E7EB

                // Update MahApps theme
                try
                {
                    ControlzEx.Theming.ThemeManager.Current.ChangeTheme(Application.Current, "Light.Teal");
                }
                catch { }
            }
            else
            {
                // Dark mode colors (Default)
                Application.Current.Resources["MainBgColor"] = Color.FromRgb(13, 17, 23); // #0D1117
                Application.Current.Resources["SidebarBgColor"] = Color.FromRgb(22, 27, 34); // #161B22
                Application.Current.Resources["CardBgColor"] = Color.FromRgb(28, 35, 51); // #1C2333
                Application.Current.Resources["TextPrimaryColor"] = Color.FromRgb(255, 255, 255); // #FFFFFF
                Application.Current.Resources["TextMutedColor"] = Color.FromRgb(139, 148, 158); // #8B949E
                Application.Current.Resources["BorderColor"] = Color.FromRgb(45, 55, 72); // #2D3748

                // Update MahApps theme
                try
                {
                    ControlzEx.Theming.ThemeManager.Current.ChangeTheme(Application.Current, "Dark.Teal");
                }
                catch { }
            }
        }

        private static string GetSystemTheme()
        {
            try
            {
                using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                {
                    var value = key?.GetValue("AppsUseLightTheme");
                    if (value is int i && i == 1)
                    {
                        return "Light";
                    }
                }
            }
            catch { }
            return "Dark"; // Fallback to Dark
        }
    }
}
