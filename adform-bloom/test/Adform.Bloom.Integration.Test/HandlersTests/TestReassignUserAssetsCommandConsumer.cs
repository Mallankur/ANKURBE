using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Integration.Test.Consumers;
using Adform.Bloom.Messages.Commands.AssetsReassignment;
using EasyNetQ.AutoSubscribe;

namespace Adform.Bloom.Integration.Test.HandlersTests
{
    public class TestReassignUserAssetsCommandConsumer : IConsumeAsync<ReassignUserAssetsCommand>
    {
        public MessagesSink<ReassignUserAssetsCommand> MessageSink;

        public TestReassignUserAssetsCommandConsumer()
        {
            MessageSink = new MessagesSink<ReassignUserAssetsCommand>(1);
        }

        public TestReassignUserAssetsCommandConsumer(int maxCount)
        {
            MessageSink = new MessagesSink<ReassignUserAssetsCommand>(maxCount);
        }

        public async Task ConsumeAsync(ReassignUserAssetsCommand message, CancellationToken cancellationToken = default)
        {
            MessageSink.Receive(message);
        }
    }
}