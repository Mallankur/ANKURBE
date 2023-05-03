using Adform.Bloom.Api.Graph.Feature;
using Adform.Bloom.DataAccess.Interfaces;
using Adform.Ciam.OngDb.Core.Interfaces;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Domain.Entities;
using Xunit;

namespace Adform.Bloom.Unit.Test.Api.Graph.Types
{
    public class FeatureTypeTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        public async Task Multiple_LicensedFeature_Requests_Triggers_AdminGraphRepository_Once(int requestCount)
        {
            var repositoryMock = new Mock<IDataLoaderRepository>();
            var batchScheduler = new Test.Common.ManualBatchScheduler();
            var loader = new LicensedFeatureByFeatureIdBatchDataLoader(repositoryMock.Object, batchScheduler);
            var requests = new Task<Contracts.Output.LicensedFeature>[requestCount];

            for (var i = 0; i < requestCount; i++)
            {
                requests[i] = Task.Factory.StartNew(async () => 
                            await loader.LoadAsync(Guid.NewGuid(), CancellationToken.None),
                        TaskCreationOptions.RunContinuationsAsynchronously)
                    .Unwrap();
            }

            while (requests.Any(task => !task.IsCompleted))
            {
                await Task.Delay(25);
                batchScheduler.Dispatch();
            }

            // assert
            var responses = await Task.WhenAll(requests);

            repositoryMock.Verify(
                x =>
                    x.GetNodesWithConnectedAsync<Feature,
                        Bloom.Domain.Entities.LicensedFeature>(
                        It.IsAny<IEnumerable<Guid>>(), It.IsAny<ILink>()),
                requestCount > 0 ? Times.Once() : Times.Never());
        }


    }
}