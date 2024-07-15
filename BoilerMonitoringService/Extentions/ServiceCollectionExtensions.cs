using BoilerMonitoringService.Application.Interfaces;
using BoilerMonitoringService.Application.Services;
using BoilerMonitoringService.Consumers;
using BoilerMonitoringService.Infrastructure;
using BoilerMonitoringService.Infrastructure.Configuration;
using MassTransit;
using Serilog;
using BoilerMonitoringService.Application.Clients;

namespace BoilerMonitoringService.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCustomServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<BoilerApiSettings>(configuration.GetSection("BoilerApi"));
            services.Configure<TemperatureThresholdsSettings>(configuration.GetSection("TemperatureThresholds"));

            services.AddTransient<ITemperatureService, TemperatureService>();
            services.AddTransient<IBoilerApiClient, BoilerApiClient>();

            services.AddHttpClient<IBoilerApiClient, BoilerApiClient>();

            services.AddMassTransit(x =>
            {
                x.SetKebabCaseEndpointNameFormatter();

                x.AddConsumer<TemperatureRequestConsumer>();

                x.UsingRabbitMq((context, cfg) =>
                {
                    var s = configuration["RabbitMq:Host"];

                    cfg.Host(configuration["RabbitMq:Host"], "/", h =>
                    {
                        h.Username(configuration["RabbitMq:Username"]);
                        h.Password(configuration["RabbitMq:Password"]);
                    });

                    cfg.ReceiveEndpoint(configuration["RabbitMq:Queue"], e =>
                    {
                        e.ConfigureConsumer<TemperatureRequestConsumer>(context);
                    });
                });
            });

            return services;
        }

        public static IHostBuilder AddCustomLogging(this IHostBuilder hostBuilder, IConfiguration configuration)
        {
            return hostBuilder.UseSerilog((context, services, configuration) => configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .WriteTo.Console());
        }
    }
}