using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Application.Abstractions.Persistence;
using Adform.Bloom.Application.Handlers;
using Adform.Bloom.Application.Queries;
using Adform.Bloom.Application.Validators;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Adform.Bloom.Runtime.Read.Test.Handlers
{
    public class RuntimeQueryHandlerTests
    {
        private Random _random;

        public RuntimeQueryHandlerTests()
        {
            _random = new Random();
        }

        [Theory]
        [MemberData(nameof(Common.Data), MemberType = typeof(Common))]
        public async Task Handler_When_Subject_Belongs_To_RootTenant(Common.TestData data)
        {
            // Arrange
            data.Input.InheritanceEnabled = true;
            var test = _random.Next(0, 1);
            if (test == 1)
                data.Input.TenantIds = new List<Guid>();

            var tenantProvider = new Mock<IAdformTenantProvider>();
            tenantProvider.Setup(o => o.GetAdformTenant(It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(data.Input.TenantIds.First());
            var runtimeProvider = new Mock<IRuntimeProvider>();
            runtimeProvider.Setup(o => o.GetSubjectEvaluation(It.IsAny<SubjectRuntimeQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(data.Output);
            var validation = new Mock<IValidateQuery>();

            // Act
            var handler = new RuntimeQueryHandler(NullLogger<RuntimeQueryHandler>.Instance,
                tenantProvider.Object,
                runtimeProvider.Object,
                validation.Object);
            var result = await handler.Handle(data.Input, CancellationToken.None);

            // Assert
            tenantProvider.Verify(o =>
                    o.GetAdformTenant(It.Is<Guid>(p => p == data.Input.SubjectId), It.IsAny<CancellationToken>()),
                Times.Once);
            runtimeProvider.Verify(o =>
                o.GetSubjectEvaluation(It.Is<SubjectRuntimeQuery>(p =>
                    !p.InheritanceEnabled
                    && p.PolicyNames == data.Input.PolicyNames
                    && p.TenantIds == (test == 0 ? data.Input.TenantIds : new[] {data.Input.TenantIds.First()})
                    && p.TenantType == data.Input.TenantType
                    && p.TenantLegacyIds == data.Input.TenantLegacyIds), It.IsAny<CancellationToken>()), Times.Once);
            Assert.True(result.SequenceEqual(data.Output));
        }


        [Theory]
        [MemberData(nameof(Common.Data), MemberType = typeof(Common))]
        public async Task Handler_When_Subject_Does_Not_Belongs_To_RootTenant(Common.TestData data)
        {
            // Arrange
            data.Input.InheritanceEnabled = true;
            var tenantProvider = new Mock<IAdformTenantProvider>();
            tenantProvider.Setup(o => o.GetAdformTenant(It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Guid.Empty);
            var runtimeProvider = new Mock<IRuntimeProvider>();
            runtimeProvider.Setup(o =>
                    o.GetSubjectEvaluation(It.IsAny<SubjectRuntimeQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(data.Output);
            var validation = new Mock<IValidateQuery>();

            // Act
            var handler = new RuntimeQueryHandler(NullLogger<RuntimeQueryHandler>.Instance,
                tenantProvider.Object,
                runtimeProvider.Object,
                validation.Object);
            var result = await handler.Handle(data.Input, CancellationToken.None);

            // Assert
            tenantProvider.Verify(o =>
                    o.GetAdformTenant(It.Is<Guid>(p => p == data.Input.SubjectId), It.IsAny<CancellationToken>()),
                Times.Once);
            runtimeProvider.Verify(o =>
                o.GetSubjectEvaluation(It.Is<SubjectRuntimeQuery>(p =>
                    p.InheritanceEnabled
                    && p.PolicyNames == data.Input.PolicyNames
                    && p.TenantIds == data.Input.TenantIds
                    && p.TenantType == data.Input.TenantType
                    && p.TenantLegacyIds == data.Input.TenantLegacyIds), It.IsAny<CancellationToken>()), Times.Once);
            Assert.True(result.SequenceEqual(data.Output));
        }
    }
}