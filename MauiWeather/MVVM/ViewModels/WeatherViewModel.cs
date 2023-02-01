using MauiWeather.MVVM.Models;
using PropertyChanged;
using System.Text.Json;
using System.Windows.Input;

namespace MauiWeather.MVVM.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class WeatherViewModel
    {
        public WeatherData WeatherData { get; set; }
        public string PlaceName { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;

        public bool IsVisible { get; set; }
        public bool IsLoading { get; set; }

        private HttpClient client;

        public WeatherViewModel() 
        {
            client= new HttpClient();
        }

        public ICommand SearchCommand =>
            new Command(async (searchText) =>
            {
                //var location =await GetCoordinatesAsync(searchText.ToString());
                PlaceName = searchText.ToString();
                await GetWeather();
            });

        private async Task GetWeather()
        {
            var url =
                $"https://api.open-meteo.com/v1/forecast?latitude=52.52&longitude=13.41&daily=weathercode,temperature_2m_max,temperature_2m_min&current_weather=true&timezone=America%2FChicago";

            IsLoading = true;

            var response =
                await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                using (var responseStream = await response.Content.ReadAsStreamAsync())
                {
                    var data = await JsonSerializer
                        .DeserializeAsync<WeatherData>(responseStream);  
                    WeatherData = data;

                    for (int i = 0; i < WeatherData.daily.time.Length; i++)
                    {
                        WeatherData.daily2s.Add(new Daily2
                        {
                            time = WeatherData.daily.time[i],
                            weathercode = WeatherData.daily.weathercode[i],
                            temperature_2m_max = WeatherData.daily.temperature_2m_max[i],
                            temperature_2m_min = WeatherData.daily.temperature_2m_min[i]
                        });
                    }
                    IsVisible= true;
                }
            }
            IsLoading= false;
        }
        private async Task<Location> GetCoordinatesAsync(string address)
        {
            IEnumerable<Location> locations = await Geocoding
                 .Default.GetLocationsAsync(address);

            Location location = locations?.FirstOrDefault();

            if (location != null)
                Console
                     .WriteLine($"Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude: {location.Altitude}");
            return location;
        }
    }
}