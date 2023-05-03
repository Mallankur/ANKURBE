using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Common.Test;
using Adform.Bloom.Contracts.Input;
using Adform.Bloom.DataAccess.Interfaces;
using Adform.Bloom.DataAccess.Providers.ReadModel;
using Adform.Bloom.Infrastructure.Models;
using Adform.Bloom.Read.Handlers;
using Adform.Bloom.Read.Queries;
using Adform.Bloom.Runtime.Contracts.Response;
using Adform.Bloom.Runtime.Contracts.Services;
using Adform.Ciam.ExceptionHandling.Abstractions.Exceptions;
using Xunit;
using Subject = Adform.Bloom.Contracts.Output.Subject;

namespace Adform.Bloom.Integration.Test.HandlersTests
{
    [Collection(nameof(HandlersCollection))]
    public class PermissionBusinessAccountRangeQueryHandlerTests : IClassFixture<TestsFixture>
    {
        private readonly PermissionBusinessAccountRangeQueryHandler _handler;
        private readonly IBloomRuntimeClient _runtimeClient;
        private readonly IBusinessAccountReadModelProvider _readModelProvider;
        private readonly IVisibilityProvider<QueryParamsTenantIds, Subject> _visibilityProvider;
        private readonly IAdminGraphRepository _repository;

        public PermissionBusinessAccountRangeQueryHandlerTests(TestsFixture fixture)
        {
            _readModelProvider = fixture.BusinessAccountReadModel;
            _runtimeClient = fixture.BloomRuntimeClient;
            _visibilityProvider = fixture.VisibilityRepositoriesContainer
                .Get<QueryParamsTenantIds, Contracts.Output.Subject>();
            _repository = fixture.GraphRepository;
            _handler = new PermissionBusinessAccountRangeQueryHandler(_runtimeClient, _readModelProvider,
                _visibilityProvider, _repository);
        }

        [Theory]
        [MemberData(nameof(PermissionBusinessAccountQueryHappyScenarioTestInput))]
        public async Task With_Given_CorrectInput_Handler_Returns_Expected_BusinessAccounts(string caseName, ClaimsPrincipal identity,
            Guid subjectId, List<string> permissionNames, EvaluationParameter evaluationParameter, List<Guid> tenantIds,
            List<Guid> expectedBusinessAccountIds)
        {
            var actualBusinessAccountIds =
                (await _handler.Handle(
                    new PermissionBusinessAccountsQuery(identity, subjectId, permissionNames, evaluationParameter,
                        tenantIds), CancellationToken.None)).Select(b => b.Id);
            Assert.True(actualBusinessAccountIds.OrderBy(id => id)
                .SequenceEqual(expectedBusinessAccountIds.OrderBy(id => id)));
        }

        [Theory]
        [MemberData(nameof(PermissionBusinessAccountQueryFailureScenarioTestInput))]
        public async Task With_Given_IncorrectInput_Handler_ThrowsError(string caseName, ClaimsPrincipal identity,
            Guid subjectId, List<string> permissionNames, EvaluationParameter evaluationParameter, List<Guid> tenantIds,
            Type errorType)
        {
            await Assert.ThrowsAsync(errorType, () => _handler.Handle(new PermissionBusinessAccountsQuery(identity,
                subjectId, permissionNames, evaluationParameter,
                tenantIds), CancellationToken.None));
        }

        private static readonly ClaimsPrincipal _adminPrincipal = Common.Test.Common.BuildUser(new RuntimeResponse[]
        {
            new()
            {
                Roles = new List<string> {Graph.AdformAdminRoleName},
                TenantId = Guid.Parse(Graph.Tenant1)
            },
            new()
            {
                Roles = new List<string> {Graph.AdformAdminRoleName},
                TenantId = Guid.Parse(Graph.Tenant2)
            },
            new()
            {
                Roles = new List<string> {Graph.AdformAdminRoleName},
                TenantId = Guid.Parse(Graph.Tenant3)
            },
            new()
            {
                Roles = new List<string> {Graph.AdformAdminRoleName},
                TenantId = Guid.Parse(Graph.Tenant4)
            },
            new()
            {
                Roles = new List<string> {Graph.AdformAdminRoleName},
                TenantId = Guid.Parse(Graph.Tenant5)
            },
            new()
            {
                Roles = new List<string> {Graph.AdformAdminRoleName},
                TenantId = Guid.Parse(Graph.Tenant6)
            },
            new()
            {
                Roles = new List<string> {Graph.AdformAdminRoleName},
                TenantId = Guid.Parse(Graph.Tenant7)
            },
            new()
            {
                Roles = new List<string> {Graph.AdformAdminRoleName},
                TenantId = Guid.Parse(Graph.Tenant11)
            }
        });

        public static TheoryData<string, ClaimsPrincipal, Guid, List<string>, EvaluationParameter, List<Guid>, List<Guid>>
            PermissionBusinessAccountQueryHappyScenarioTestInput()
        {
            var data = new TheoryData<string, ClaimsPrincipal, Guid, List<string>, EvaluationParameter, List<Guid>, List<Guid>>
            {
                {
                    "Case 0",
                    _adminPrincipal,
                    Guid.Parse(Graph.Subject0),
                    new List<string>
                    {
                        Graph.Permission0Name
                    },
                    EvaluationParameter.All,
                    null,
                    new[]{Graph.Tenant0}.ToGuids()
                },
                {
                    "Case 1",
                    _adminPrincipal,
                    Guid.Parse(Graph.Subject4),
                    new List<string>
                    {
                        Graph.Permission7Name, Graph.Permission8Name
                    },
                    EvaluationParameter.All,
                    null,
                    new[]{Graph.Tenant7}.ToGuids()
                },
                {
                    "Case 2",
                    _adminPrincipal,
                    Guid.Parse(Graph.Subject3),
                    new List<string>
                    {
                        Graph.Permission1Name, Graph.Permission2Name, Graph.Permission3Name, Graph.Permission4Name
                    },
                    EvaluationParameter.All,
                    null,
                    new[]{Graph.Tenant2, Graph.Tenant9}.ToGuids()
                },
                {
                    "Case 3",
                    _adminPrincipal,
                    Guid.Parse(Graph.Subject5),
                    new List<string>
                    {
                        Graph.Permission11Name, Graph.Permission12Name
                    },
                    EvaluationParameter.Any,
                    null,
                    new[]{Graph.Tenant3, Graph.Tenant10, Graph.Tenant11}.ToGuids()
                },
                {
                    "Case 4",
                    _adminPrincipal,
                    Guid.Parse(Graph.Subject4),
                    new List<string>
                    {
                        "NonExistentPermission"
                    },
                    EvaluationParameter.Any,
                    null,
                    new List<Guid>()
                },
                {
                    "Case 5",
                    _adminPrincipal,
                    Guid.Parse(Graph.Subject4),
                    new List<string>(),
                    EvaluationParameter.Any,
                    null,
                    new List<Guid>()
                },
                {
                    "Case 6",
                    _adminPrincipal,
                    Guid.Parse(Graph.Subject5),
                    new List<string>
                    {
                        Graph.Permission11Name, Graph.Permission12Name
                    },
                    EvaluationParameter.Any,
                    new []{Graph.Tenant3}.ToGuids(),
                    new[]{Graph.Tenant3}.ToGuids()
                },
                {
                    "Case 7",
                    Common.Test.Common.BuildUser(new RuntimeResponse[]
                    {
                        new()
                        {
                            Roles = new List<string> {Graph.CustomRole7Name},
                            TenantId = Guid.Parse(Graph.Tenant1)
                        },
                        new()
                        {
                            Roles = new List<string> {Graph.CustomRole6Name},
                            TenantId = Guid.Parse(Graph.Tenant2)
                        }
                    }),
                    Guid.Parse(Graph.Subject2),
                    new List<string>
                    {
                        Graph.Permission5Name
                    },
                    EvaluationParameter.Any,
                    null,
                    new []{ Graph.Tenant2}.ToGuids()
                },
                {
                    
                    "Case 8",
                    Common.Test.Common.BuildUser(new RuntimeResponse[]
                    {
                        new()
                        {
                            Roles = new List<string> {Graph.LocalAdminRoleName},
                            TenantId = Guid.Parse(Graph.Tenant13)
                        }
                    }),
                    Guid.Parse(Graph.Subject10),
                    new List<string>
                    {
                        Graph.Permission13Name
                    },
                    EvaluationParameter.Any,
                    null,
                    new List<Guid> {Guid.Parse(Graph.Tenant13)}
                },
                {
                    "Case 9",
                    _adminPrincipal,
                    Guid.Parse(Graph.Subject5),
                    new List<string>(),
                    EvaluationParameter.Any,
                    null,
                    new List<Guid>()
                }
            };
            return data;
        }

        public static TheoryData<string, ClaimsPrincipal, Guid, List<string>, EvaluationParameter, List<Guid>, Type>
            PermissionBusinessAccountQueryFailureScenarioTestInput()
        {
            var data = new TheoryData<string, ClaimsPrincipal, Guid, List<string>, EvaluationParameter, List<Guid>, Type>
            {
                {
                    "Case 0",
                    _adminPrincipal,
                    Guid.NewGuid(),
                    new List<string> {Graph.Permission13Name},
                    EvaluationParameter.All,
                    null,
                    typeof(NotFoundException)
                },
                {
                    "Case 1",
                    Common.Test.Common.BuildUser(new RuntimeResponse[]
                    {
                        new()
                        {
                            Roles = new List<string> {Graph.CustomRole10Name},
                            TenantId = Guid.Parse(Graph.Tenant1)
                        }
                    }),
                    Guid.Parse(Graph.Subject6),
                    new List<string>
                    {
                        Graph.Permission13Name
                    },
                    EvaluationParameter.Any,
                    null,
                    typeof(ForbiddenException)
                },
            };
            return data;
        }
    }
}