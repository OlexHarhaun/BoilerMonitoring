using BoilerMonitoringService.Infrastructure;
using BoilerMonitoringService.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BoilerMonitoringService.Application.Clients
{
 public class BoilerApiClient : IBoilerApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly ILogger<BoilerApiClient> _logger;

        public BoilerApiClient(HttpClient httpClient, IOptions<BoilerApiSettings> settings, ILogger<BoilerApiClient> logger)
        {
            _httpClient = httpClient;
            _baseUrl = settings.Value.BaseUrl;
            _logger = logger;
        }

        public async Task<double> GetTemperatureAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching temperature from Boiler API.");
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/temperature", cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    var temperatureFahrenheit = await response.Content.ReadAsStringAsync(cancellationToken);
                    var temperature = double.Parse(temperatureFahrenheit);
                    _logger.LogInformation($"Fetched temperature: {temperature}F");
                    return temperature;
                }
                else
                {
                    _logger.LogError($"Failed to fetch temperature. Status code: {response.StatusCode}");
                    // Handle the failure scenario, throw an exception, or return a default value.
                    throw new HttpRequestException($"Failed to fetch temperature. Status code: {response.StatusCode}");
                }
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                _logger.LogError(ex, "Error fetching temperature from Boiler API.");
                throw;
            }
        }
    }
}
