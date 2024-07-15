namespace BoilerMonitoringService.Contracts.Responses
{
    public class UnusualTemperatureDetected
    {
        public string RequestId { get; set; }
        public double TemperatureCelsius { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
