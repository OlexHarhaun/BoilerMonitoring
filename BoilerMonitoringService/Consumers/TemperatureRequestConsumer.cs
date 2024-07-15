using BoilerMonitoringService.Application.Interfaces;
using BoilerMonitoringService.Contracts.Requests;
using BoilerMonitoringService.Contracts.Responses;
using MassTransit;

namespace BoilerMonitoringService.Consumers
{
    public class TemperatureRequestConsumer : IConsumer<GetTemperatureRequest>
    {
        private readonly ITemperatureService _temperatureService;
        private readonly ILogger<TemperatureRequestConsumer> _logger;

        public TemperatureRequestConsumer(ITemperatureService temperatureService, ILogger<TemperatureRequestConsumer> logger)
        {
            _temperatureService = temperatureService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<GetTemperatureRequest> context)
        {
            var requestId = context.Message.RequestId;
            _logger.LogInformation($"[RequestId: {requestId}] Received temperature request message.");
            try
            {
                var temperatureCelsius = await _temperatureService.GetTemperatureAsync(context.CancellationToken, requestId);
                await context.RespondAsync(new GetTemperatureResponse
                {
                    RequestId = requestId,
                    TemperatureCelsius = temperatureCelsius
                });
                _logger.LogInformation($"[RequestId: {requestId}] Responded with temperature.");
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                _logger.LogError(ex, $"[RequestId: {requestId}] Error processing temperature request.");
                throw;
            }
        }
    }
}