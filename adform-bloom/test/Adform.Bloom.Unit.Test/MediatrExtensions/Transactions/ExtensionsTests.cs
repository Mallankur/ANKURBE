using System;
using System.Linq;
using Adform.Bloom.Mediatr.Extensions;
using Adform.Bloom.Unit.Test.MediatrExtensions.Structures;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace Adform.Bloom.Unit.Test.MediatrExtensions.Transactions
{
    public class ExtensionsTests
    {
        [Fact]
        public void RegisterTransactionalBehaviors_PostfixIsProvided_BehaviorsShouldBeRegistered()
        {
            var c = new ServiceCollection
            {
                new ServiceDescriptor(typeof(IRequestHandler<FakeRequest, FakeResponse>),
                    typeof(FakeRequestHandler), ServiceLifetime.Scoped),
                new ServiceDescriptor(typeof(IRequestHandler<FakeRequest2, FakeResponse2>),
                    typeof(FakeRequest2Handler), ServiceLifetime.Scoped),
                new ServiceDescriptor(typeof(IRequestHandler<FakeRequest3, MediatR.Unit>),
                    typeof(FakeRequest3Handler), ServiceLifetime.Scoped)
            };

            c.RegisterTransactionalBehaviors("FakeRequest");
            Assert.True(6 == c.Count);

            var des = c.SingleOrDefault(d => d.Lifetime == ServiceLifetime.Singleton &&
                                             d.ServiceType == typeof(IPipelineBehavior<FakeRequest, FakeResponse>) &&
                                             d.ImplementationType ==
                                             typeof(UnitOfWorkBehavior<FakeRequest, FakeResponse>) &&
                                             d.ImplementationFactory == null &&
                                             d.ImplementationInstance == null);
            Assert.NotNull(des);

            var des2 = c.SingleOrDefault(d => d.Lifetime == ServiceLifetime.Singleton &&
                                              d.ServiceType == typeof(IPipelineBehavior<FakeRequest2, FakeResponse2>) &&
                                              d.ImplementationType ==
                                              typeof(UnitOfWorkBehavior<FakeRequest2, FakeResponse2>) &&
                                              d.ImplementationFactory == null &&
                                              d.ImplementationInstance == null);
            Assert.NotNull(des2);

            var des3 = c.SingleOrDefault(d => d.Lifetime == ServiceLifetime.Singleton &&
                                              d.ServiceType == typeof(IPipelineBehavior<FakeRequest3, MediatR.Unit>) &&
                                              d.ImplementationType ==
                                              typeof(UnitOfWorkBehavior<FakeRequest3, MediatR.Unit>) &&
                                              d.ImplementationFactory == null &&
                                              d.ImplementationInstance == null);
            Assert.NotNull(des3);
        }

        [Fact]
        public void RegisterTransactionalBehaviors_IncorrectPostfixIsProvided_BehaviorsShouldNotBeRegistered()
        {
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
                () => c.RegisterTransactionalBehaviors("incorrect"));
            Assert.True(3 == c.Count);

            var des = c.SingleOrDefault(d => d.Lifetime == ServiceLifetime.Scoped &&
                                             d.ServiceType == typeof(IPipelineBehavior<FakeRequest, FakeResponse>) &&
                                             d.ImplementationType ==
                                             typeof(UnitOfWorkBehavior<FakeRequest, FakeResponse>) &&
                                             d.ImplementationFactory == null &&
                                             d.ImplementationInstance == null);
            Assert.Null(des);

            var des2 = c.SingleOrDefault(d => d.Lifetime == ServiceLifetime.Scoped &&
                                              d.ServiceType == typeof(IPipelineBehavior<FakeRequest2, FakeResponse2>) &&
                                              d.ImplementationType ==
                                              typeof(UnitOfWorkBehavior<FakeRequest2, FakeResponse2>) &&
                                              d.ImplementationFactory == null &&
                                              d.ImplementationInstance == null);
            Assert.Null(des2);

            var des3 = c.SingleOrDefault(d => d.Lifetime == ServiceLifetime.Scoped &&
                                              d.ServiceType == typeof(IPipelineBehavior<FakeRequest3, MediatR.Unit>) &&
                                              d.ImplementationType ==
                                              typeof(UnitOfWorkBehavior<FakeRequest3, MediatR.Unit>) &&
                                              d.ImplementationFactory == null &&
                                              d.ImplementationInstance == null);
            Assert.Null(des3);
        }
    }
}