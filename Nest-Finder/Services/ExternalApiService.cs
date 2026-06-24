using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NestFinder.Services
{
    public class ExternalApiService
    {
        private static readonly HttpClient _httpClient;

        static ExternalApiService()
        {
            _httpClient = new HttpClient();
            // Set User-Agent headers as some APIs require them (e.g. Open-Meteo geocoding)
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("NestFinder/1.0 (Windows; C# WPF App)");
        }

        public async Task<decimal?> GetUsdToPkrExchangeRateAsync()
        {
            try
            {
                string url = "https://open.er-api.com/v6/latest/USD";
                var response = await _httpClient.GetStringAsync(url);
                var data = JsonSerializer.Deserialize<ExchangeRateResponse>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (data != null && data.Rates != null && data.Rates.TryGetValue("PKR", out decimal pkrRate))
                {
                    return pkrRate;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching exchange rate: {ex.Message}");
            }
            return null;
        }

        public async Task<WeatherData?> GetWeatherForCityAsync(string city)
        {
            if (string.IsNullOrWhiteSpace(city)) return null;

            try
            {
                // Step 1: Geocoding (Convert city name to lat/lon coordinates)
                string geocodeUrl = $"https://geocoding-api.open-meteo.com/v1/search?name={Uri.EscapeDataString(city)}&count=1&language=en&format=json";
                var geocodeResponse = await _httpClient.GetStringAsync(geocodeUrl);
                var geocodeData = JsonSerializer.Deserialize<GeocodingResponse>(geocodeResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (geocodeData == null || geocodeData.Results == null || geocodeData.Results.Count == 0)
                {
                    return null;
                }

                var location = geocodeData.Results[0];

                // Step 2: Weather Forecast (Get current weather by lat/lon)
                string weatherUrl = $"https://api.open-meteo.com/v1/forecast?latitude={location.Latitude}&longitude={location.Longitude}&current=temperature_2m,relative_humidity_2m,apparent_temperature,weather_code,wind_speed_10m";
                var weatherResponse = await _httpClient.GetStringAsync(weatherUrl);
                var weatherData = JsonSerializer.Deserialize<WeatherResponse>(weatherResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (weatherData != null && weatherData.Current != null)
                {
                    var current = weatherData.Current;
                    var (desc, icon) = GetWeatherVisuals(current.Weather_Code);
                    return new WeatherData
                    {
                        Temperature = current.Temperature_2M,
                        ApparentTemperature = current.Apparent_Temperature,
                        Humidity = current.Relative_Humidity_2M,
                        WindSpeed = current.Wind_Speed_10M,
                        Description = desc,
                        Icon = icon
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching weather: {ex.Message}");
            }
            return null;
        }

        private static (string Description, string Icon) GetWeatherVisuals(int code)
        {
            return code switch
            {
                0 => ("Clear Sky", "Sun"),
                1 => ("Mainly Clear", "CloudSun"),
                2 => ("Partly Cloudy", "CloudSun"),
                3 => ("Overcast", "Cloud"),
                45 or 48 => ("Foggy", "Smog"),
                51 or 53 or 55 => ("Drizzle", "CloudRain"),
                61 or 63 or 65 => ("Rainy", "CloudShowersHeavy"),
                71 or 73 or 75 => ("Snowy", "Snowflake"),
                80 or 81 or 82 => ("Rain Showers", "CloudShowersWater"),
                95 or 96 or 99 => ("Thunderstorm", "CloudBolt"),
                _ => ("Cloudy", "Cloud")
            };
        }
    }

    public class WeatherData
    {
        public double Temperature { get; set; }
        public double ApparentTemperature { get; set; }
        public double Humidity { get; set; }
        public double WindSpeed { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
    }

    public class ExchangeRateResponse
    {
        public string Result { get; set; } = string.Empty;
        [JsonPropertyName("base_code")]
        public string BaseCode { get; set; } = string.Empty;
        public Dictionary<string, decimal> Rates { get; set; } = new();
    }

    public class GeocodingResponse
    {
        public List<GeocodingResult>? Results { get; set; }
    }

    public class GeocodingResult
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class WeatherResponse
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public WeatherCurrent? Current { get; set; }
    }

    public class WeatherCurrent
    {
        [JsonPropertyName("temperature_2m")]
        public double Temperature_2M { get; set; }

        [JsonPropertyName("apparent_temperature")]
        public double Apparent_Temperature { get; set; }

        [JsonPropertyName("relative_humidity_2m")]
        public double Relative_Humidity_2M { get; set; }

        [JsonPropertyName("wind_speed_10m")]
        public double Wind_Speed_10M { get; set; }

        [JsonPropertyName("weather_code")]
        public int Weather_Code { get; set; }
    }
}
