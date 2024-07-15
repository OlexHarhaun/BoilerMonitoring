namespace BoilerMonitoringService.Contracts.Responses
{
    public class GetTemperatureResponse
    {
        public string RequestId { get; set; }
        public double TemperatureCelsius { get; set; }
    }
}
