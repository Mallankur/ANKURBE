using System;

namespace Adform.Bloom.Messages.Events
{
    public class SubjectDisabledEvent
    {
        public Guid SubjectId { get; set; }
        public Guid ActorId { get; set; }
        public Guid CorrelationId { get; set; }
        public long Timestamp { get; set; }
    }    
}
