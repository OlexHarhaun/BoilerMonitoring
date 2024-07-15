using BoilerMonitoringService.Application.Helpers;
using BoilerMonitoringService.Application.Interfaces;
using BoilerMonitoringService.Contracts.Responses;
using BoilerMonitoringService.Infrastructure;
using BoilerMonitoringService.Infrastructure.Configuration;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BoilerMonitoringService.Application.Services
{
    public class TemperatureService : ITemperatureService
    {
        private readonly IBoilerApiClient _boilerApiClient;
        private readonly ILogger<TemperatureService> _logger;
        private readonly TemperatureThresholdsSettings _thresholdsSettings;
        private readonly IPublishEndpoint _publishEndpoint;

        public TemperatureService(IBoilerApiClient boilerApiClient, ILogger<TemperatureService> logger, IOptions<TemperatureThresholdsSettings> thresholdsSettings, IPublishEndpoint publishEndpoint)
        {
            _boilerApiClient = boilerApiClient;
            _logger = logger;
            _thresholdsSettings = thresholdsSettings.Value;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<double> GetTemperatureAsync(CancellationToken cancellationToken, string requestId)
        {
            _logger.LogInformation($"[RequestId: {requestId}] Getting temperature...");
            try
            {
                var temperatureFahrenheit = await _boilerApiClient.GetTemperatureAsync(cancellationToken);
                var temperatureCelsius = TemperatureConverter.FahrenheitToCelcius(temperatureFahrenheit);
                _logger.LogInformation($"Converted temperature: {temperatureCelsius}C");

                if (temperatureCelsius < _thresholdsSettings.MinTemperatureCelsius || temperatureCelsius > _thresholdsSettings.MaxTemperatureCelsius)
                {
                    _logger.LogError($"[RequestId: {requestId}] Unusual temperature detected: {temperatureCelsius} Celsius");
                    await _publishEndpoint.Publish(new UnusualTemperatureDetected
                    {
                        RequestId = requestId,
                        TemperatureCelsius = temperatureCelsius,
                        Timestamp = DateTime.UtcNow
                    });
                }

                return temperatureCelsius;
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                _logger.LogError(ex, "Error getting temperature from TemperatureService.");
                throw;
            }
        }
    }
}
