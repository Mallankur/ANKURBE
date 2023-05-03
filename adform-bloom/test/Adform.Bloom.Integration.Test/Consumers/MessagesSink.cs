using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Adform.Bloom.Integration.Test.Consumers
{
    
    public class MessagesSink<T>
    {
        private readonly TaskCompletionSource<object> _allMessagedReceived = new TaskCompletionSource<object>();
        private readonly object locker = new object();
        private readonly int maxCount;
        private readonly List<T> receivedMessages = new List<T>();

        public MessagesSink(int maxCount)
        {
            this.maxCount = maxCount;
            if (maxCount == 0)
                _allMessagedReceived.TrySetResult(null);
        }

        public IReadOnlyList<T> ReceivedMessages
        {
            get
            {
                lock (locker)
                {
                    return receivedMessages.ToList<T>();
                }
            }
        }

        public async Task WaitAllReceivedAsync(CancellationToken cancellationToken = default)
        {
            await using (
                cancellationToken.Register(
                    x => ((TaskCompletionSource<object>)x)?.TrySetCanceled(),
                    _allMessagedReceived,
                    false
                )
            )
            {
                await _allMessagedReceived.Task;
            }
        }

        public void Receive(T message)
        {
            lock (locker)
            {
                if (receivedMessages.Count >= maxCount)
                    throw new InvalidOperationException();

                receivedMessages.Add(message);
                if (receivedMessages.Count == maxCount)
                    _allMessagedReceived.TrySetResult(null);
            }
        }
    }
}