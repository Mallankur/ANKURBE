using System;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Api.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Adform.Bloom.Unit.Test.Api.Services
{
    public class ReadModelHealthCheckTests
    {
        [Fact]
        public async Task CheckHealthAsync_ReadModelIsHealthy_ReturnsHealthy()
        {
            //Arrange

            var logger = NullLogger<ReadModelHealthCheck>.Instance;
            var client = new Mock<IReadModelClient>();
            client.Setup(c => c.IsHealthy()).ReturnsAsync(true);
            var check = new ReadModelHealthCheck(logger, client.Object);

            //Act
            var report = await check.CheckHealthAsync(new HealthCheckContext(), CancellationToken.None);

            //Assert
            Assert.Null(report.Exception);
            Assert.Equal(HealthStatus.Healthy, report.Status);
        }

        [Fact]
        public async Task CheckHealthAsync_BloomRuntimeIsNotAccessible_ReturnsHealthy()
        {
            //Arrange
            var logger = NullLogger<ReadModelHealthCheck>.Instance;
            var client = new Mock<IReadModelClient>();
            var ex = new Exception();
            client.Setup(c => c.IsHealthy()).ThrowsAsync(ex);
            var check = new ReadModelHealthCheck(logger, client.Object);

            //Act
            var report = await check.CheckHealthAsync(new HealthCheckContext(), CancellationToken.None);

            //Assert
            Assert.NotNull(report.Exception);
            Assert.Equal(ex, report.Exception);
            Assert.Equal(HealthStatus.Unhealthy, report.Status);
        }


        [Fact]
        public async Task CheckHealthAsync_BloomRuntimeIsNotHealthy_ReturnsUnhealthy()
        {
            //Arrange

            var logger = NullLogger<ReadModelHealthCheck>.Instance;
            var client = new Mock<IReadModelClient>();
            client.Setup(c => c.IsHealthy()).ReturnsAsync(false);
            var check = new ReadModelHealthCheck(logger, client.Object);

            //Act
            var report = await check.CheckHealthAsync(new HealthCheckContext(), CancellationToken.None);

            //Assert
            Assert.Null(report.Exception);
            Assert.Equal(HealthStatus.Unhealthy, report.Status);
        }
    }
}
