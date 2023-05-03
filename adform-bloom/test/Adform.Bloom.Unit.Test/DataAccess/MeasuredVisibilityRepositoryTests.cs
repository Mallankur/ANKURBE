using Adform.Bloom.DataAccess.Decorators;
using Adform.Bloom.DataAccess.Interfaces;
using Adform.Ciam.Monitoring.Abstractions.CustomStructures;
using Moq;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Adform.Bloom.Infrastructure.Models;
using Xunit;
using AutoFixture;

namespace Adform.Bloom.Unit.Test.DataAccess
{
    public class MeasuredVisibilityRepositoryTests : TestClassBase
    {
        public MeasuredVisibilityRepositoryTests()
        {
            _repo = new MeasuredVisibilityProvider<QueryParams, Entity>(_inner.Object, _histogramMock.Object);
        }

        private readonly Mock<ICustomHistogram> _histogramMock = new Mock<ICustomHistogram>();

        private readonly Mock<IVisibilityProvider<QueryParams, Entity>> _inner =
            new Mock<IVisibilityProvider<QueryParams, Entity>>();

        private readonly MeasuredVisibilityProvider<QueryParams, Entity> _repo;

        public class Entity
        {
        }

        [Fact]
        public async Task Repo_Calls_EvaluateAccessAsync_With_Paging()
        {
            await _repo.EvaluateVisibilityAsync(new ClaimsPrincipal(), new QueryParams(), 1, 2);
            _inner.Verify(m => 
                    m.EvaluateVisibilityAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<QueryParams>(), 1, 2),
                Times.Once);
        }

        [Fact]
        public async Task Repo_Calls_HasAccessAsync()
        {
            await _repo.HasItemVisibilityAsync(new ClaimsPrincipal(), Guid.NewGuid());
            _inner.Verify(m => m.HasItemVisibilityAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<Guid>(), null), Times.Once);
        }

        [Fact]
        public async Task Repo_Calls_GetAccessibleResourcesAsync()
        {
            var principal = Fixture.Create<ClaimsPrincipal>();
            var resourceIds = Fixture.CreateMany<Guid>().ToList();
            var tenantIds = Fixture.CreateMany<Guid>().ToList();
            var label = Fixture.Create<string>();

            var filter = new QueryParamsTenantIds
            {
                ResourceIds = resourceIds,
                TenantIds = tenantIds
            };
            await _repo.GetVisibleResourcesAsync(principal, filter, label);

            _inner.Verify(m => m.GetVisibleResourcesAsync(principal, filter, label));
        }
    }
}