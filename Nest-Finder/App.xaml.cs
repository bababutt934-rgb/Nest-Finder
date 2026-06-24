using System.Windows;

namespace NestFinder
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            NestFinder.Helpers.ThemeHelper.Initialize();
            
            // Force database initialization on startup
            try
            {
                using var conn = NestFinder.Services.DbHelper.GetConnection();
                conn.Open();
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Initial database connection failed: {ex.Message}");
            }
        }
    }
}
