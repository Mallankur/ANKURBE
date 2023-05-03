using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Common.Test;
using Adform.Bloom.Domain.Entities;
using Adform.Bloom.Infrastructure.Models;
using Adform.Bloom.Read.Handlers;
using Adform.Bloom.Read.Queries;
using Adform.Bloom.Runtime.Contracts.Response;
using Adform.Ciam.ExceptionHandling.Abstractions.Exceptions;
using MapsterMapper;
using Xunit;

namespace Adform.Bloom.Integration.Test.HandlersTests
{
    [Collection(nameof(HandlersCollection))]
    public class BusinessAccountFilteredSingleQueryHandlerTests : IClassFixture<TestsFixture>
    {
        private readonly TestsFixture _fixture;
        private readonly BusinessAccountSingleQueryHandler _queryHandler;

        public BusinessAccountFilteredSingleQueryHandlerTests(TestsFixture fixture)
        {
            _fixture = fixture;
            _queryHandler = new BusinessAccountSingleQueryHandler(fixture.GraphRepository,
                fixture.VisibilityRepositoriesContainer.Get<QueryParamsBusinessAccount, Contracts.Output.Tenant>(),
                fixture.BusinessAccountReadModel,
                new Mapper());
        }

        [Theory]
        [MemberData(nameof(BusinessAccountResult))]
        public async Task Get_BusinessAccount_Returns_Expected_Result(ClaimsPrincipal identity,
            Tenant tenant, bool hasException)
        {
            if (!hasException)
            {
                var businessAccount = await _queryHandler.Handle(new BusinessAccountQuery(identity, tenant.Id),
                    CancellationToken.None);

                Assert.Equal(businessAccount.Id, tenant.Id);
            }
            else
            {
                await Assert.ThrowsAsync<ForbiddenException>(() =>
                    _queryHandler.Handle(new BusinessAccountQuery(identity, tenant.Id), CancellationToken.None));
            }
        }

        public static TheoryData<ClaimsPrincipal, Tenant, bool> BusinessAccountResult()

        {
            var data = new TheoryData<ClaimsPrincipal, Tenant, bool>();
            data.Add(Common.Test.Common.BuildUser(new RuntimeResponse
            {
                TenantId = Guid.Parse(Graph.Tenant1),
                TenantName = Graph.Tenant1Name,
                Roles = new[] {Graph.OtherRole},
                Permissions = new[] {""}
            }), new Tenant
            {
                Id = Guid.Parse(Graph.Tenant1),
            }, false);

            data.Add(Common.Test.Common.BuildUser(new RuntimeResponse
            {
                TenantId = Guid.Parse(Graph.Tenant4),
                TenantName = Graph.Tenant4Name,
                Roles = new[] {Graph.OtherRole},
                Permissions = new[] {""}
            }), new Tenant
            {
                Id = Guid.Parse(Graph.Tenant3),
            }, true);

            return data;
        }
    }
}