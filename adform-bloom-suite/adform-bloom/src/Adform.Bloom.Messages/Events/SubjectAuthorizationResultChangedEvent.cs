using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Adform.Bloom.Messages.Events
{
    [DataContract]
    public class SubjectAuthorizationResultChangedEvent
    {
        [DataMember(Name = "master_account_id", Order = 1)]
        public string SubjectId { get; set; }
        [DataMember(Name = "authorization", Order = 2)]
        public IEnumerable<AuthorizationResult> Authorization { get; set; } = new List<AuthorizationResult>();
        [DataMember(Name = "event_id", Order = 3)]
        public string EventId { get; set; } = Guid.NewGuid().ToString();
        [DataMember(Name = "timestamp_ms", Order = 4)]
        public long Timestamp { get; set; }
    }
}