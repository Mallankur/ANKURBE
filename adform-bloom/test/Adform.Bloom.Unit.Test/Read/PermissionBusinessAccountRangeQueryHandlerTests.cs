using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Contracts.Input;
using Adform.Bloom.Contracts.Output;
using Adform.Bloom.DataAccess.Interfaces;
using Adform.Bloom.DataAccess.Providers.ReadModel;
using Adform.Bloom.Infrastructure.Models;
using Adform.Bloom.Read.Handlers;
using Adform.Bloom.Read.Queries;
using Adform.Bloom.Runtime.Contracts.Request;
using Adform.Bloom.Runtime.Contracts.Response;
using Adform.Bloom.Runtime.Contracts.Services;
using Adform.Ciam.ExceptionHandling.Abstractions.Exceptions;
using Adform.Ciam.SharedKernel.Entities;
using Moq;
using Xunit;
using Subject = Adform.Bloom.Contracts.Output.Subject;

namespace Adform.Bloom.Unit.Test.Read
{
    public class PermissionBusinessAccountRangeQueryHandlerTests
    {
        private readonly PermissionBusinessAccountRangeQueryHandler _handler;

        private readonly Mock<IBloomRuntimeClient> _runtimeClientMock = new();

        private readonly Mock<IBusinessAccountReadModelProvider> _readModelProviderMock = new();

        private readonly Mock<IVisibilityProvider<QueryParamsTenantIds, Subject>> _visibilityProviderMock = new();

        private readonly Mock<IAdminGraphRepository> _repositoryMock = new();

        public PermissionBusinessAccountRangeQueryHandlerTests()
        {
            _handler = new PermissionBusinessAccountRangeQueryHandler(_runtimeClientMock.Object,
                _readModelProviderMock.Object, _visibilityProviderMock.Object, _repositoryMock.Object);
        }

        [Fact]
        public async Task Given_NonExistentSubject_Throws_NotFound()
        {
            // Arrange
            var principal = new ClaimsPrincipal();
            var subjectId = Guid.NewGuid();
            _repositoryMock
                .Setup(r => r.GetNodeAsync<Bloom.Domain.Entities.Subject>(entity => entity.Id == subjectId))
                .ReturnsAsync((Bloom.Domain.Entities.Subject?) null);

            // Act
            await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(new PermissionBusinessAccountsQuery(
                principal, subjectId,
                new List<string>(),
                EvaluationParameter.Any), CancellationToken.None));

            // Assert
            _repositoryMock.Verify(r
                => r.GetNodeAsync<Bloom.Domain.Entities.Subject>(entity => entity.Id == subjectId), Times.Once);
            _visibilityProviderMock.Verify(v => v.HasItemVisibilityAsync(principal, subjectId, null), Times.Never);
            _runtimeClientMock.Verify(
                e => e.InvokeAsync(It.IsAny<SubjectRuntimeRequest>(), It.IsAny<CancellationToken>()), Times.Never);
            _readModelProviderMock.Verify(
                e => e.SearchForResourcesAsync(0, It.IsAny<int>(), null, It.IsAny<IEnumerable<Guid>>(), null,
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Given_ActorWithoutVisibility_Throws_Forbidden()
        {
            // Arrange
            var principal = new ClaimsPrincipal();
            var subjectId = Guid.NewGuid();
            _repositoryMock
                .Setup(r => r.GetNodeAsync<Bloom.Domain.Entities.Subject>(entity => entity.Id == subjectId))
                .ReturnsAsync(new Bloom.Domain.Entities.Subject());
            _visibilityProviderMock.Setup(v => v.HasItemVisibilityAsync(principal, subjectId, null)).ReturnsAsync(false);

            // Act
            await Assert.ThrowsAsync<ForbiddenException>(() => _handler.Handle(new PermissionBusinessAccountsQuery(
                principal, subjectId,
                new List<string>(),
                EvaluationParameter.Any), CancellationToken.None));

            // Assert
            _repositoryMock.Verify(r
                => r.GetNodeAsync<Bloom.Domain.Entities.Subject>(entity => entity.Id == subjectId), Times.Once);
            _visibilityProviderMock.Verify(v => v.HasItemVisibilityAsync(principal, subjectId, null), Times.Once);
            _runtimeClientMock.Verify(
                e => e.InvokeAsync(It.IsAny<SubjectRuntimeRequest>(), It.IsAny<CancellationToken>()), Times.Never);
            _readModelProviderMock.Verify(
                e => e.SearchForResourcesAsync(0, It.IsAny<int>(), null, It.IsAny<IEnumerable<Guid>>(), null,
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Runtime_Invoke_Called_Once()
        {
            // Arrange
            var principal = new ClaimsPrincipal();
            var subjectId = Guid.NewGuid();
            _repositoryMock
                .Setup(r => r.GetNodeAsync<Bloom.Domain.Entities.Subject>(entity => entity.Id == subjectId))
                .ReturnsAsync(new Bloom.Domain.Entities.Subject());
            _visibilityProviderMock.Setup(v => v.HasItemVisibilityAsync(principal, subjectId, null)).ReturnsAsync(true);
            _runtimeClientMock
                .Setup(r => r.InvokeAsync(It.IsAny<SubjectRuntimeRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<RuntimeResponse>(0));

            // Act
            await _handler.Handle(new PermissionBusinessAccountsQuery(principal, subjectId,
                new List<string>(),
                EvaluationParameter.Any), CancellationToken.None);

            // Assert
            _runtimeClientMock.Verify(
                e => e.InvokeAsync(It.IsAny<SubjectRuntimeRequest>(), It.IsAny<CancellationToken>()), Times.Once);
            _repositoryMock.Verify(r
                => r.GetNodeAsync<Bloom.Domain.Entities.Subject>(entity => entity.Id == subjectId), Times.Once);
            _visibilityProviderMock.Verify(v => v.HasItemVisibilityAsync(principal, subjectId, null), Times.Once);
            _readModelProviderMock.Verify(
                e => e.SearchForResourcesAsync(0, It.IsAny<int>(), null, It.IsAny<IEnumerable<Guid>>(), null,
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Given_Empty_Runtime_Result_ReadModel_Called_Never()
        {
            // Arrange
            var principal = new ClaimsPrincipal();
            var subjectId = Guid.NewGuid();
            var ids = new List<RuntimeResponse>(0).Select(r => r.TenantId).ToList();
            _repositoryMock
                .Setup(r => r.GetNodeAsync<Bloom.Domain.Entities.Subject>(entity => entity.Id == subjectId))
                .ReturnsAsync(new Bloom.Domain.Entities.Subject());
            _visibilityProviderMock.Setup(v => v.HasItemVisibilityAsync(principal, subjectId, null)).ReturnsAsync(true);
            _runtimeClientMock
                .Setup(r => r.InvokeAsync(It.IsAny<SubjectRuntimeRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<RuntimeResponse>(0));
            _readModelProviderMock
                .Setup(r => r.SearchForResourcesAsync(0, ids.Count, null, ids, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    new EntityPagination<BusinessAccount>(0, ids.Count, ids.Count, new List<BusinessAccount>()));

            // Act
            await _handler.Handle(new PermissionBusinessAccountsQuery(principal, subjectId,
                new List<string>(),
                EvaluationParameter.Any), CancellationToken.None);

            // Assert
            _repositoryMock.Verify(r
                => r.GetNodeAsync<Bloom.Domain.Entities.Subject>(entity => entity.Id == subjectId), Times.Once);
            _visibilityProviderMock.Verify(v => v.HasItemVisibilityAsync(principal, subjectId, null), Times.Once);
            _runtimeClientMock.Verify(
                e => e.InvokeAsync(It.IsAny<SubjectRuntimeRequest>(), It.IsAny<CancellationToken>()), Times.Once);
            _readModelProviderMock.Verify(
                e => e.SearchForResourcesAsync(0, ids.Count, null, ids, null, It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Given_Null_ReadModelResult_Throws_NotFound()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var principal = new ClaimsPrincipal();
            var subjectId = Guid.NewGuid();
            var permissionNames = new List<string> {"PermissionA"};
            var policyResults = new List<RuntimeResponse> {new() {TenantId = tenantId, Permissions = permissionNames}};

            var ids = policyResults.Select(r => r.TenantId).ToList();
            _repositoryMock
                .Setup(r => r.GetNodeAsync<Bloom.Domain.Entities.Subject>(entity => entity.Id == subjectId))
                .ReturnsAsync(new Bloom.Domain.Entities.Subject());
            _visibilityProviderMock.Setup(v => v.HasItemVisibilityAsync(principal, subjectId, null)).ReturnsAsync(true);
            _runtimeClientMock
                .Setup(r => r.InvokeAsync(It.IsAny<SubjectRuntimeRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(policyResults);
            _readModelProviderMock
                .Setup(r => r.SearchForResourcesAsync(0, ids.Count, null, ids, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync((EntityPagination<BusinessAccount>) null);

            // Act
            await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(new PermissionBusinessAccountsQuery(
                principal, subjectId,
                permissionNames,
                EvaluationParameter.Any), CancellationToken.None));

            // Assert
            _repositoryMock.Verify(r
                => r.GetNodeAsync<Bloom.Domain.Entities.Subject>(entity => entity.Id == subjectId), Times.Once);
            _visibilityProviderMock.Verify(v => v.HasItemVisibilityAsync(principal, subjectId, null), Times.Once);
            _runtimeClientMock.Verify(
                e => e.InvokeAsync(It.IsAny<SubjectRuntimeRequest>(), It.IsAny<CancellationToken>()), Times.Once);
            _readModelProviderMock.Verify(
                e => e.SearchForResourcesAsync(0, ids.Count, It.IsAny<QueryParams>(), ids, null, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Given_NonExistent_In_ReadModel_Throws_NotFound()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var principal = new ClaimsPrincipal();
            var subjectId = Guid.NewGuid();
            var permissionNames = new List<string> {"PermissionA"};
            var policyResults = new List<RuntimeResponse> {new() {TenantId = tenantId, Permissions = permissionNames}};
            var ids = policyResults.Select(r => r.TenantId).ToList();
            var readModelResult = new EntityPagination<BusinessAccount>(0, int.MaxValue, 0, null);
            _repositoryMock
                .Setup(r => r.GetNodeAsync<Bloom.Domain.Entities.Subject>(entity => entity.Id == subjectId))
                .ReturnsAsync(new Bloom.Domain.Entities.Subject());
            _visibilityProviderMock.Setup(v => v.HasItemVisibilityAsync(principal, subjectId, null)).ReturnsAsync(true);
            _runtimeClientMock
                .Setup(r => r.InvokeAsync(It.IsAny<SubjectRuntimeRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(policyResults);
            _readModelProviderMock
                .Setup(r => r.SearchForResourcesAsync(0, ids.Count, null, ids, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(readModelResult);

            // Act
            await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(new PermissionBusinessAccountsQuery(
                principal, subjectId,
                permissionNames,
                EvaluationParameter.Any), CancellationToken.None));

            // Assert
            _repositoryMock.Verify(r
                => r.GetNodeAsync<Bloom.Domain.Entities.Subject>(entity => entity.Id == subjectId), Times.Once);
            _visibilityProviderMock.Verify(v => v.HasItemVisibilityAsync(principal, subjectId, null), Times.Once);
            _runtimeClientMock.Verify(
                e => e.InvokeAsync(It.IsAny<SubjectRuntimeRequest>(), It.IsAny<CancellationToken>()), Times.Once);
            _readModelProviderMock.Verify(
                e => e.SearchForResourcesAsync(0, ids.Count, It.IsAny<QueryParams>(), ids, null, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Theory]
        [ClassData(typeof(InputData))]
        public async Task Given_Correct_Input_ReadModel_Called_With_Expected_Ids(List<string> wantedPermissions,
            List<RuntimeResponse> runtimeResults, EvaluationParameter evaluationParameter, List<Guid> tenantIds,
            List<Guid> expectedIds)
        {
            // Arrange
            var principal = new ClaimsPrincipal();
            var subjectId = Guid.NewGuid();
            var expectedResult = expectedIds.Select(id => new BusinessAccount {Id = id}).ToList();
            _repositoryMock
                .Setup(r => r.GetNodeAsync<Bloom.Domain.Entities.Subject>(entity => entity.Id == subjectId))
                .ReturnsAsync(new Bloom.Domain.Entities.Subject());
            _visibilityProviderMock.Setup(v => v.HasItemVisibilityAsync(principal, subjectId, null)).ReturnsAsync(true);
            _runtimeClientMock
                .Setup(r => r.InvokeAsync(It.IsAny<SubjectRuntimeRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(tenantIds == null
                    ? runtimeResults
                    : runtimeResults.Where(r => tenantIds.Contains(r.TenantId)));
            _readModelProviderMock
                .Setup(r => r.SearchForResourcesAsync(0, expectedIds.Count, It.IsAny<QueryParams>(), expectedIds, null,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new EntityPagination<BusinessAccount>(0, expectedIds.Count, expectedIds.Count,
                    expectedResult));

            // Act
            var actualResult =
                await _handler.Handle(
                    new PermissionBusinessAccountsQuery(principal, subjectId, wantedPermissions,
                        evaluationParameter, tenantIds), CancellationToken.None);

            // Assert
            _readModelProviderMock.Verify(
                e => e.SearchForResourcesAsync(0, expectedIds.Count, It.IsAny<QueryParams>(), expectedIds, null,
                    It.IsAny<CancellationToken>()), expectedIds.Any() ? Times.Once : Times.Never);
            Assert.True(actualResult.SequenceEqual(expectedResult));
        }


        public class InputData : IEnumerable<object[]>
        {
            private const string PermissionA = "PermissionA";
            private const string PermissionB = "PermissionB";
            private readonly Guid _businessAccountAId = Guid.NewGuid();
            private readonly Guid _businessAccountBId = Guid.NewGuid();

            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[]
                {
                    new List<string> {PermissionA},
                    new List<RuntimeResponse>
                        {new() {Permissions = new List<string> {PermissionA}, TenantId = _businessAccountAId}},
                    EvaluationParameter.All,
                    null,
                    new List<Guid> {_businessAccountAId}
                };
                yield return new object[]
                {
                    new List<string>(),
                    new List<RuntimeResponse>
                        {new() {Permissions = new List<string> {PermissionA}, TenantId = _businessAccountAId}},
                    EvaluationParameter.All,
                    null,
                    new List<Guid>()
                };
                yield return new object[]
                {
                    new List<string> {PermissionA},
                    new List<RuntimeResponse>
                    {
                        new() {Permissions = new List<string> {PermissionA}, TenantId = _businessAccountAId},
                        new() {Permissions = new List<string> {PermissionA}, TenantId = _businessAccountBId}
                    },
                    EvaluationParameter.All,
                    null,
                    new List<Guid> {_businessAccountAId, _businessAccountBId}
                };
                yield return new object[]
                {
                    new List<string> {PermissionB},
                    new List<RuntimeResponse>
                    {
                        new() {Permissions = new List<string> {PermissionA}, TenantId = _businessAccountAId},
                        new() {Permissions = new List<string> {PermissionA}, TenantId = _businessAccountBId}
                    },
                    EvaluationParameter.All,
                    null,
                    new List<Guid>()
                };
                yield return new object[]
                {
                    new List<string> {PermissionB, PermissionA},
                    new List<RuntimeResponse>
                    {
                        new() {Permissions = new List<string> {PermissionA}, TenantId = _businessAccountAId},
                        new() {Permissions = new List<string> {PermissionA}, TenantId = _businessAccountBId}
                    },
                    EvaluationParameter.All,
                    null,
                    new List<Guid>()
                };
                yield return new object[]
                {
                    new List<string> {PermissionB, PermissionA},
                    new List<RuntimeResponse>
                    {
                        new() {Permissions = new List<string> {PermissionA}, TenantId = _businessAccountAId},
                        new() {Permissions = new List<string> {PermissionA}, TenantId = _businessAccountBId}
                    },
                    EvaluationParameter.Any,
                    null,
                    new List<Guid> {_businessAccountAId, _businessAccountBId}
                };
                yield return new object[]
                {
                    new List<string> {PermissionB, PermissionA},
                    new List<RuntimeResponse>
                    {
                        new() {Permissions = new List<string> {PermissionA}, TenantId = _businessAccountAId},
                        new() {Permissions = new List<string> {PermissionA}, TenantId = _businessAccountBId}
                    },
                    EvaluationParameter.Any,
                    new List<Guid> {_businessAccountBId},
                    new List<Guid> {_businessAccountBId}
                };
                yield return new object[]
                {
                    new List<string> {PermissionB, PermissionA},
                    new List<RuntimeResponse>
                    {
                        new()
                        {
                            Permissions = new List<string> {PermissionA, PermissionB}, TenantId = _businessAccountAId
                        },
                        new() {Permissions = new List<string> {PermissionA}, TenantId = _businessAccountBId}
                    },
                    EvaluationParameter.Any,
                    null,
                    new List<Guid> {_businessAccountAId, _businessAccountBId}
                };
                yield return new object[]
                {
                    new List<string> {PermissionB, PermissionA},
                    new List<RuntimeResponse>
                    {
                        new()
                        {
                            Permissions = new List<string> {PermissionA, PermissionB}, TenantId = _businessAccountAId
                        },
                        new() {Permissions = new List<string> {PermissionA}, TenantId = _businessAccountBId}
                    },
                    EvaluationParameter.All,
                    null,
                    new List<Guid> {_businessAccountAId}
                };
                yield return new object[]
                {
                    new List<string> {PermissionB, PermissionA},
                    new List<RuntimeResponse>
                    {
                        new()
                        {
                            Permissions = new List<string> {PermissionA, PermissionB}, TenantId = _businessAccountAId
                        },
                        new()
                        {
                            Permissions = new List<string> {PermissionA, PermissionB}, TenantId = _businessAccountBId
                        }
                    },
                    EvaluationParameter.All,
                    null,
                    new List<Guid> {_businessAccountAId, _businessAccountBId}
                };
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}