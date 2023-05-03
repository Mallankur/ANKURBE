using Adform.Bloom.Infrastructure.Models;
using Adform.Bloom.Read.Handlers;
using Adform.Bloom.Read.Queries;
using Adform.Ciam.ExceptionHandling.Abstractions.Exceptions;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Common.Test;
using Adform.Bloom.Domain.Entities;
using Adform.Bloom.Runtime.Contracts.Response;
using Xunit;

namespace Adform.Bloom.Integration.Test.HandlersTests
{
    [Collection(nameof(HandlersCollection))]
    public class UserFilteredSingleQueryHandlerTests : IClassFixture<TestsFixture>
    {
        private readonly TestsFixture _fixture;
        private readonly UserSingleQueryHandler _queryHandler;

        public UserFilteredSingleQueryHandlerTests(TestsFixture fixture)
        {
            _fixture = fixture;
            _queryHandler = new UserSingleQueryHandler(fixture.GraphRepository, 
                fixture.VisibilityRepositoriesContainer.Get<QueryParamsTenantIds, Contracts.Output.Subject>(),
                fixture.UserReadModel);
        }

        [Theory]
        [MemberData(nameof(SubjectResult))]
        public async Task Get_Subject__Returns_Expected_Result(ClaimsPrincipal identity,
            Subject subject, bool hasException)
        {
            if (!hasException)
            {
                var subjectResult = await _queryHandler.Handle(new UserQuery(identity, subject.Id), CancellationToken.None);

                Assert.Equal(subjectResult.Id, subject.Id);
            }
            else
            {
                await Assert.ThrowsAsync<ForbiddenException>(() => _queryHandler.Handle(new UserQuery(identity, subject.Id), CancellationToken.None));
            }
        }

        [Fact]
        public async Task Get_Trafficker_Returns_NotFound_Result()
        {
            var identity = Common.Test.Common.BuildUser(new RuntimeResponse
            {
                TenantId = Guid.Parse(Graph.Tenant0),
                TenantName = Graph.Tenant0Name,
                Roles = new[] {Graph.AdformAdmin},
                Permissions = new[] {""}
            });

            var traffickerId = Guid.Parse(Graph.Trafficker0);
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _queryHandler.Handle(new UserQuery(identity, traffickerId), CancellationToken.None));
        }

        public static TheoryData<ClaimsPrincipal, Subject, bool> SubjectResult()

        {
            var data = new TheoryData<ClaimsPrincipal, Subject, bool>();
            data.Add(Common.Test.Common.BuildUser(new RuntimeResponse
            {
                TenantId = Guid.Parse(Graph.Tenant2),
                TenantName = Graph.Tenant2Name,
                Roles = new[] { Graph.OtherRole },
                Permissions = new[] { "" }
            }, Graph.Subject3), new Subject
            {
                Id = Guid.Parse(Graph.Subject2),
            }, false);

            data.Add(Common.Test.Common.BuildUser(new RuntimeResponse
            {
                TenantId = Guid.Parse(Graph.Tenant4),
                TenantName = Graph.Tenant4Name,
                Roles = new[] { Graph.OtherRole },
                Permissions = new[] { "" }
            }), new Subject
            {
                Id = Guid.Parse(Graph.Subject0),
            }, true);

            return data;
        }
    }
}