using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Adform.Bloom.Contracts.Output;
using Adform.Bloom.DataAccess.Interfaces;
using Adform.Bloom.Infrastructure.Models;
using Adform.Ciam.SharedKernel.Entities;
using Xunit;

namespace Adform.Bloom.Unit.Test.DataAccess
{
    public class IVisibilityProviderTests
    {
        [Fact]
        public async Task HasAccessAsync_Returns_Same_Result_For_Default_Parameters()
        {
            var resourceId = Guid.NewGuid();
            const string label = "label";
            IVisibilityProvider<QueryParams, BaseNodeDto> repo = new FakeRepository(resourceId, label);
            Assert.True(await repo.HasItemVisibilityAsync(Common.BuildPrincipal(), resourceId, label));
        }

        private class FakeRepository : IVisibilityProvider<QueryParams, BaseNodeDto>
        {
            private readonly Guid _expectedResourceId;
            private readonly string _expectedLabel;

            public FakeRepository(Guid expectedResourceId, string expectedLabel)
            {
                _expectedResourceId = expectedResourceId;
                _expectedLabel = expectedLabel;
            }

            public Task<bool> HasVisibilityAsync(ClaimsPrincipal subject, QueryParams filter, string label = null) =>
                Task.FromResult(filter.ResourceIds.Contains(_expectedResourceId) &&
                                label == _expectedLabel);

            public Task<IEnumerable<Guid>> GetVisibleResourcesAsync(ClaimsPrincipal subject, QueryParams filter,
                string? label = null)
            {
                throw new NotImplementedException();
            }

            public Task<EntityPagination<BaseNodeDto>> EvaluateVisibilityAsync(ClaimsPrincipal subject,
                QueryParams filter, int skip, int limit)
            {
                throw new NotImplementedException();
            }
        }
    }
}