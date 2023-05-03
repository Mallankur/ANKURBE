using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Adform.Bloom.Messages.Events
{
    [DataContract]
    public class SubjectAssignmentsNotification
    {
        [DataMember(Name = "master_account_id", Order = 1)]
        public string SubjectId { get; set; }
        [DataMember(Name = "business_account_id", Order = 2)]
        public string TenantId { get; set; }
        [DataMember(Name = "business_account_name", Order = 3)]
        public string TenantName { get; set; }
        [DataMember(Name = "role_assignments", Order = 4)]
        public IEnumerable<string> Assignments { get; set; }
        [DataMember(Name = "role_unassignments", Order = 5)]
        public IEnumerable<string> Unassignments { get; set; }
        [DataMember(Name = "event_id", Order = 6)]
        public string EventId { get; set; } = Guid.NewGuid().ToString();
        [DataMember(Name = "timestamp_ms", Order = 7)]
        public long Timestamp { get; set; }
    }
}