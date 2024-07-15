namespace BoilerMonitoringService.Application.Helpers
{
    public static class TemperatureConverter
    {
        public static double FahrenheitToCelcius(double temperature)
        {
            var temperatureCelsius = Math.Round(((temperature - 32) * 5 / 9), 2);
            return temperatureCelsius;
        }
    }
}
