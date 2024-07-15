using BoilerMonitoringService.Application.Interfaces;
using BoilerMonitoringService.Consumers;
using MassTransit;
using Microsoft.AspNetCore.Mvc;

namespace BoilerMonitoringService
{
    [ApiController]
    public class controller : ControllerBase
    {
        private readonly ITemperatureService _temperatureService;

        public controller(ITemperatureService temperatureService)
        {
            _temperatureService = temperatureService;
        }

        //[HttpGet("t")]
        //public async Task<double> Get()
        //{
        //    var r = await _temperatureService.GetTemperatureAsync(CancellationToken new);

        //    return r;
        //}
    }
}
