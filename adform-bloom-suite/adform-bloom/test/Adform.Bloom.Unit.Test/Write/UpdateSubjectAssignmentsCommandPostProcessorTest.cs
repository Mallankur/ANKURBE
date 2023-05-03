using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Domain.Entities;
using Adform.Bloom.Infrastructure.Cache;
using Adform.Bloom.Runtime.Contracts.Request;
using Adform.Bloom.Runtime.Contracts.Response;
using Adform.Bloom.Runtime.Contracts.Services;
using Adform.Bloom.Write.Commands;
using Adform.Bloom.Write.PostProcessors;
using Adform.Bloom.Write.Services;
using AutoFixture;
using Moq;
using Xunit;

namespace Adform.Bloom.Unit.Test.Write;

public class UpdateSubjectAssignmentsCommandPostProcessorTest
{
    private readonly Mock<INotificationService> _notificationService = new Mock<INotificationService>();
    private readonly Mock<IBloomRuntimeClient> _client = new Mock<IBloomRuntimeClient>();
    private readonly Mock<IBloomCacheManager> _cacheManager = new Mock<IBloomCacheManager>();
    private readonly UpdateSubjectAssignmentsCommandPostProcessor _postProcessor;
    private readonly Fixture _fixture;
    public UpdateSubjectAssignmentsCommandPostProcessorTest()
    {

        _postProcessor = new UpdateSubjectAssignmentsCommandPostProcessor(_notificationService.Object, _cacheManager.Object, _client.Object);
        _fixture = new Fixture();
    }

    [Fact]
    public async Task Process_Calls_Underlying_Services_Once()
    {
        // Arrange ----------------------------------------------------------------------------------------------------
        var tenantId = Guid.NewGuid();
        var subjectId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var assignment = new List<RoleTenant>
        {
            new RoleTenant
            {
                RoleId = roleId,
                TenantId = tenantId
            }
        };

        var principal = Common.BuildPrincipal(tenantId.ToString());
        var direct = _fixture.Create<IEnumerable<RuntimeResponse>>();
        var originalState = _fixture.Create<IEnumerable<RuntimeResponse>>();
        var request = new UpdateSubjectAssignmentsCommand(principal, subjectId, null, assignment);

        _client.Setup(o => o.InvokeAsync(It.IsAny<SubjectRuntimeRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(direct);

        // Act ---------------------------------------------------------------------------------------------------------
        await _postProcessor.Process(request, originalState, CancellationToken.None);

        // Assert ------------------------------------------------------------------------------------------------------
        _notificationService.Verify(
            p => p.SendEvents(It.IsAny<UpdateSubjectAssignmentsCommand>(),
                originalState,
                direct,
                It.IsAny<CancellationToken>()), Times.Once);
        _notificationService.Verify(
            p => p.SendNotifications(It.IsAny<UpdateSubjectAssignmentsCommand>(),
                originalState,
                direct,
                It.IsAny<IEnumerable<RuntimeResponse>>(),
                It.IsAny<CancellationToken>()), Times.Once);
        _cacheManager.Verify(
            p => p.ForgetBySubjectAsync(It.Is<string>(p => p == subjectId.ToString()), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}