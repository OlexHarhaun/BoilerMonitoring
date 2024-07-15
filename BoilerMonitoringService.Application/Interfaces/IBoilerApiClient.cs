namespace BoilerMonitoringService.Infrastructure
{
    public interface IBoilerApiClient
    {
        Task<double> GetTemperatureAsync(CancellationToken cancellationToken);
    }
}
