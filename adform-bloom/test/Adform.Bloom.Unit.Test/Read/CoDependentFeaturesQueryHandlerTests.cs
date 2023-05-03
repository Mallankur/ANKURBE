using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.DataAccess.Interfaces;
using Adform.Bloom.Domain.Entities;
using Adform.Bloom.Read.Handlers;
using Adform.Bloom.Read.Queries;
using Adform.Ciam.OngDb.Core.Interfaces;
using Moq;
using Xunit;

namespace Adform.Bloom.Unit.Test.Read
{
    public class CoDependentFeaturesQueryHandlerTests
    {
        private readonly Mock<IDataLoaderRepository> _dataLoaderRepositoryMock = new Mock<IDataLoaderRepository>();
        private readonly CoDependentFeaturesQueryHandler _handler;

        public CoDependentFeaturesQueryHandlerTests()
        {
            _handler = new CoDependentFeaturesQueryHandler(_dataLoaderRepositoryMock.Object);
        }

        [Fact]
        public async Task CoDependentFeaturesQueryHandler_Returns_Expected_Result()
        {
            var id = Guid.NewGuid();
            _dataLoaderRepositoryMock
                .Setup(x => x.GetNodesWithConnectedAsync<Feature, Feature>(It.IsAny<IEnumerable<Guid>>(),
                    It.IsAny<ILink>())).ReturnsAsync(new List<ConnectedEntity<Feature>>
                {
                    new ConnectedEntity<Feature>
                    {
                        StartNodeId = id,
                        ConnectedNode = new Feature("feature1")
                    },
                    new ConnectedEntity<Feature>
                    {
                        StartNodeId = id,
                        ConnectedNode = new Feature("feature2")
                    }
                });

            var result = await _handler.Handle(new CoDependentFeaturesQuery(new List<Guid>()), CancellationToken.None);

            Assert.True(result[id].Select(x => x.Name).SequenceEqual(new[] {"feature1", "feature2"}));
        }
    }
}