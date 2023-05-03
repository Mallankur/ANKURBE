using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Api.Graph.Policy;
using Adform.Bloom.DataAccess.Interfaces;
using Adform.Ciam.OngDb.Core.Interfaces;
using Moq;
using Xunit;
using Role = Adform.Bloom.Domain.Entities.Role;

namespace Adform.Bloom.Unit.Test.Api.Graph.Types
{
    public class PolicyTypeTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        public async Task Multiple_Roles_Requests_Triggers_AdminGraphRepository_Once(int requestCount)
        {
            var repositoryMock = new Mock<IDataLoaderRepository>();
            var batchScheduler = new Test.Common.ManualBatchScheduler();
            var loader = new RolesByPolicyIdBatchDataLoader(repositoryMock.Object, batchScheduler);
            var requests = new Task<Contracts.Output.Role[]>[requestCount];

            for (var i = 0; i < requestCount; i++)
            {
                requests[i] = Task.Factory.StartNew(async () => await loader.LoadAsync(Guid.NewGuid(), CancellationToken.None),
                        TaskCreationOptions.RunContinuationsAsynchronously).Unwrap();
            }

            while (requests.Any(task => !task.IsCompleted))
            {
                await Task.Delay(25);
                batchScheduler.Dispatch();
            }

            repositoryMock.Verify(
                x =>
                    x.GetNodesWithConnectedAsync<Bloom.Domain.Entities.Policy, Role>(
                        It.IsAny<IEnumerable<Guid>>(), It.IsAny<ILink>()),
                requestCount > 0 ? Times.Once() : Times.Never());
        }
    }
}