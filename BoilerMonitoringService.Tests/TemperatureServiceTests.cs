using BoilerMonitoringService.Application.Interfaces;
using BoilerMonitoringService.Application.Services;
using BoilerMonitoringService.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using BoilerMonitoringService.Infrastructure;
using BoilerMonitoringService.Consumers;
using BoilerMonitoringService.Contracts.Requests;
using MassTransit;

namespace BoilerMonitoringService.Tests
{
    public class TemperatureServiceTests
    {
        private readonly Mock<IBoilerApiClient> _boilerApiClientMock;
        private readonly Mock<ILogger<TemperatureService>> _loggerMock;
        private readonly IOptions<TemperatureThresholdsSettings> _thresholdsSettings;
        private readonly TemperatureService _temperatureService;
        private readonly Mock<IPublishEndpoint> _publishEndpoint;

        public TemperatureServiceTests()
        {
            _boilerApiClientMock = new Mock<IBoilerApiClient>();
            _loggerMock = new Mock<ILogger<TemperatureService>>();
            _thresholdsSettings = Options.Create(new TemperatureThresholdsSettings
            {
                MinTemperatureCelsius = 0,
                MaxTemperatureCelsius = 100
            });
            _publishEndpoint = new Mock<IPublishEndpoint>();

            _temperatureService = new TemperatureService(
                _boilerApiClientMock.Object,
                _loggerMock.Object,
                _thresholdsSettings,
                _publishEndpoint.Object);
        }

        [Fact]
        public async Task GetTemperatureAsync_ShouldLogError_WhenTemperatureIsOutOfRange()
        {
            // Arrange
            var mockApiClient = new Mock<IBoilerApiClient>();
            var publishEndpoint = new Mock<IPublishEndpoint>();
            var mockLogger = new Mock<ILogger<TemperatureService>>();
            var mockOptions = Options.Create(new TemperatureThresholdsSettings { MinTemperatureCelsius = 10, MaxTemperatureCelsius = 90 });
            var requstId = "test-request-id";

            mockApiClient.Setup(x => x.GetTemperatureAsync(It.IsAny<CancellationToken>())).ReturnsAsync(5);

            var temperatureService = new TemperatureService(mockApiClient.Object, mockLogger.Object, mockOptions, publishEndpoint.Object);

            // Act
            await temperatureService.GetTemperatureAsync(CancellationToken.None, requstId);

            // Assert
            mockLogger.Verify(x => x.Log<It.IsAnyType>(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unusual temperature detected")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }

        [Fact]
        public async Task GetTemperatureAsync_ShouldLogError_WhenFetchingTemperatureFails()
        {
            // Arrange
            var mockApiClient = new Mock<IBoilerApiClient>();
            var publishEndpoint = new Mock<IPublishEndpoint>();
            var mockLogger = new Mock<ILogger<TemperatureService>>();
            var mockOptions = Options.Create(new TemperatureThresholdsSettings { MinTemperatureCelsius = 0, MaxTemperatureCelsius = 100 });
            var requstId = "test-request-id";

            mockApiClient.Setup(x => x.GetTemperatureAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new HttpRequestException("Simulated error"));

            var temperatureService = new TemperatureService(mockApiClient.Object, mockLogger.Object, mockOptions, publishEndpoint.Object);

            // Act  
            await Assert.ThrowsAsync<HttpRequestException>(async () => await temperatureService.GetTemperatureAsync(CancellationToken.None, requstId));

            // Assert
            mockLogger.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once); // Ensure error log is recorded once
        }

        [Fact]
        public async Task TemperatureRequestConsumer_ShouldLogError_WhenServiceFails()
        {
            // Arrange
            var mockTemperatureService = new Mock<ITemperatureService>();
            var mockLogger = new Mock<ILogger<TemperatureRequestConsumer>>();
            var requstId = "test-request-id";
            var request = new GetTemperatureRequest { RequestId = requstId };
            var cancellationToken = CancellationToken.None;

            mockTemperatureService.Setup(x => x.GetTemperatureAsync(cancellationToken, requstId)).ThrowsAsync(new InvalidOperationException("Simulated service error"));

            var consumer = new TemperatureRequestConsumer(mockTemperatureService.Object, mockLogger.Object);

            var mockConsumeContext = new Mock<ConsumeContext<GetTemperatureRequest>>();
            mockConsumeContext.SetupGet(x => x.Message).Returns(request);
            mockConsumeContext.SetupGet(x => x.CancellationToken).Returns(cancellationToken);

            // Act  
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await consumer.Consume(mockConsumeContext.Object));

            // Assert
            mockLogger.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error processing temperature request")),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once); // Ensure error log is recorded once
        }
    }
}