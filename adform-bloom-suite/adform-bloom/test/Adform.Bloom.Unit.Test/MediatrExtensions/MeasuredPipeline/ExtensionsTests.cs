using System;
using System.Linq;
using Adform.Bloom.Mediatr.Extensions;
using Adform.Bloom.Unit.Test.MediatrExtensions.Structures;
using Adform.Ciam.Monitoring.Abstractions.CustomStructures;
using Adform.Ciam.Monitoring.Abstractions.Provider;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Xunit;

namespace Adform.Bloom.Unit.Test.MediatrExtensions.MeasuredPipeline
{
    public class ExtensionsTests
    {
        [Fact]
        public void RegisterMeasuredBehaviors_PostfixIsProvided_BehaviorsShouldBeRegistered()
        {
            const string histogramName = "histogramName";
            var histogramMock = new Mock<ICustomHistogram>();
            var metricsProviderMock = new Mock<IMetricsProvider>();
            metricsProviderMock.Setup(m => m.GetHistogram(histogramName)).Returns(histogramMock.Object);
            var spMock = new Mock<IServiceProvider>();
            spMock.Setup(m => m.GetService(typeof(IMetricsProvider))).Returns(metricsProviderMock.Object);

            var c = new ServiceCollection
            {
                new ServiceDescriptor(typeof(IRequestHandler<FakeRequest, FakeResponse>),
                    typeof(FakeRequestHandler), ServiceLifetime.Scoped),
                new ServiceDescriptor(typeof(IRequestHandler<FakeRequest2, FakeResponse2>),
                    typeof(FakeRequest2Handler), ServiceLifetime.Scoped),
                new ServiceDescriptor(typeof(IRequestHandler<FakeRequest3, MediatR.Unit>),
                    typeof(FakeRequest3Handler), ServiceLifetime.Scoped)
            };

            c.RegisterMeasuredMediatrBehavior("FakeRequest", histogramName);
            Assert.True(6 == c.Count);

            var des = c.SingleOrDefault(d => d.Lifetime == ServiceLifetime.Singleton &&
                                             d.ServiceType == typeof(IPipelineBehavior<FakeRequest, FakeResponse>) &&
                                             d.ImplementationType == null &&
                                             d.ImplementationFactory.Invoke(spMock.Object) is
                                                 MeasuredPipelineBehavior<FakeRequest, FakeResponse> &&
                                             d.ImplementationInstance == null);
            Assert.NotNull(des);

            var des2 = c.SingleOrDefault(d => d.Lifetime == ServiceLifetime.Singleton &&
                                              d.ServiceType == typeof(IPipelineBehavior<FakeRequest2, FakeResponse2>) &&
                                              d.ImplementationType == null &&
                                              d.ImplementationFactory.Invoke(spMock.Object) is
                                                  MeasuredPipelineBehavior<FakeRequest2, FakeResponse2> &&
                                              d.ImplementationInstance == null);
            Assert.NotNull(des2);
            
            var des3 = c.SingleOrDefault(d => d.Lifetime == ServiceLifetime.Singleton &&
                                              d.ServiceType == typeof(IPipelineBehavior<FakeRequest3, MediatR.Unit>) &&
                                              d.ImplementationType == null &&
                                              d.ImplementationFactory.Invoke(spMock.Object) is
                                                  MeasuredPipelineBehavior<FakeRequest3, MediatR.Unit> &&
                                              d.ImplementationInstance == null);
            Assert.NotNull(des3);
        }
        
        [Fact]
        public void RegisterMeasuredBehaviors_IncorrectPostfixProvided__Exception_Is_thrown__BehaviorsShouldNotBeRegistered()
        {
            const string histogramName = "histogramName";
            var histogramMock = new Mock<ICustomHistogram>();
            var metricsProviderMock = new Mock<IMetricsProvider>();
            metricsProviderMock.Setup(m => m.GetHistogram(histogramName)).Returns(histogramMock.Object);
            var spMock = new Mock<IServiceProvider>();
            spMock.Setup(m => m.GetService(typeof(IMetricsProvider))).Returns(metricsProviderMock.Object);

            var c = new ServiceCollection
            {
                new ServiceDescriptor(typeof(IRequestHandler<FakeRequest, FakeResponse>),
                    typeof(FakeRequestHandler), ServiceLifetime.Scoped),
                new ServiceDescriptor(typeof(IRequestHandler<FakeRequest2, FakeResponse2>),
                    typeof(FakeRequest2Handler), ServiceLifetime.Scoped),
                new ServiceDescriptor(typeof(IRequestHandler<FakeRequest3, MediatR.Unit>),
                    typeof(FakeRequest3Handler), ServiceLifetime.Scoped)
            };

            Assert.Throws<InvalidOperationException>(
                () => c.RegisterMeasuredMediatrBehavior("incorrect", histogramName));
            
            Assert.True(3 == c.Count);

            var des = c.SingleOrDefault(d => d.Lifetime == ServiceLifetime.Singleton &&
                                             d.ServiceType == typeof(IPipelineBehavior<FakeRequest, FakeResponse>) &&
                                             d.ImplementationType == null &&
                                             d.ImplementationFactory.Invoke(spMock.Object) is
                                                 MeasuredPipelineBehavior<FakeRequest, FakeResponse> &&
                                             d.ImplementationInstance == null);
            Assert.Null(des);

            var des2 = c.SingleOrDefault(d => d.Lifetime == ServiceLifetime.Singleton &&
                                              d.ServiceType == typeof(IPipelineBehavior<FakeRequest2, FakeResponse2>) &&
                                              d.ImplementationType == null &&
                                              d.ImplementationFactory.Invoke(spMock.Object) is
                                                  MeasuredPipelineBehavior<FakeRequest2, FakeResponse2> &&
                                              d.ImplementationInstance == null);
            Assert.Null(des2);
            
            var des3 = c.SingleOrDefault(d => d.Lifetime == ServiceLifetime.Singleton &&
                                              d.ServiceType == typeof(IPipelineBehavior<FakeRequest3, MediatR.Unit>) &&
                                              d.ImplementationType == null &&
                                              d.ImplementationFactory.Invoke(spMock.Object) is
                                                  MeasuredPipelineBehavior<FakeRequest3, MediatR.Unit> &&
                                              d.ImplementationInstance == null);
            Assert.Null(des3);
        }
    }
}