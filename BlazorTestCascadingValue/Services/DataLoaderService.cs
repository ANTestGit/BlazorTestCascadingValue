using BlazorTestCascadingValue.Data;

namespace BlazorTestCascadingValue.Services
{
    public class DataLoaderService
    {
        private WeatherForecastService ForecastService { get; }

        public event EventHandler<WeatherForecast[]>? OnDataChanged;

        public DataLoaderService(WeatherForecastService forecastService)
        {
            ForecastService = forecastService;
        }
        public WeatherForecast[]? Data { get; private set; }

        public async Task<WeatherForecast[]?> LoadDataAsync()
        {
            Data = await ForecastService.GetForecastAsync(DateTime.Now);
            OnOnDataChanged(Data);
            return Data;
        }

        protected virtual void OnOnDataChanged(WeatherForecast[] e)
        {
            OnDataChanged?.Invoke(this, e);
        }
    }
}
