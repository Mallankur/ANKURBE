using System;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Mediatr.Extensions;
using Adform.Bloom.Unit.Test.MediatrExtensions.Structures;
using Adform.Ciam.Monitoring.Abstractions.CustomStructures;
using MediatR;
using Moq;
using Xunit;

namespace Adform.Bloom.Unit.Test.MediatrExtensions.MeasuredPipeline
{
    public class MeasuredBehaviorTests
    {
        [Fact]
        public async Task Error_Flow()
        {
            var histogramMock = new Mock<ICustomHistogram>();
            var next = new Mock<RequestHandlerDelegate<FakeResponse>>();
            next.Setup(n => n.Invoke()).ThrowsAsync(new Exception());
            var b = new MeasuredPipelineBehavior<FakeRequest, FakeResponse>(histogramMock.Object);

            await Assert.ThrowsAsync<Exception>(
                async () => await b.Handle(new FakeRequest(), CancellationToken.None, next.Object));
            
            histogramMock.Verify(m => m.Observe(It.IsAny<double>(), It.IsAny<string[]>()), Times.Once);
        }

        [Fact]
        public async Task Normal_Flow()
        {
            var histogramMock = new Mock<ICustomHistogram>();
            var response = new FakeResponse();
            var next = new Mock<RequestHandlerDelegate<FakeResponse>>();
            next.Setup(n => n.Invoke()).ReturnsAsync(response);
            var b = new MeasuredPipelineBehavior<FakeRequest, FakeResponse>(histogramMock.Object);

            var res = await b.Handle(new FakeRequest(), CancellationToken.None, next.Object);

            Assert.NotNull(res);
            Assert.Same(response, res);
            histogramMock.Verify(m => m.Observe(It.IsAny<double>(), It.IsAny<string[]>()), Times.Once);
        }
    }
}