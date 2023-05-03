using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Integration.Test.Consumers;
using Adform.Bloom.Messages.Events;
using EasyNetQ.AutoSubscribe;

namespace Adform.Bloom.Integration.Test.HandlersTests
{
    public class TestSubjectAssignedEventConsumer : IConsumeAsync<SubjectAssignmentEvent>
    {
        public MessagesSink<SubjectAssignmentEvent> MessageSink;

        public TestSubjectAssignedEventConsumer()
        {
            MessageSink = new MessagesSink<SubjectAssignmentEvent>(1);
        }
        
        public TestSubjectAssignedEventConsumer(int maxCount)
        {
            MessageSink = new MessagesSink<SubjectAssignmentEvent>(maxCount);
        }

        public async Task ConsumeAsync(SubjectAssignmentEvent message, CancellationToken cancellationToken = default)
        {
            MessageSink.Receive(message);
        }
    }
}