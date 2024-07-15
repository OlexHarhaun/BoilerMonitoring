namespace BoilerMonitoringService.Application.Interfaces
{
    public interface ITemperatureService
    {
        Task<double> GetTemperatureAsync(CancellationToken cancellationToken, string requestId);
    }
}