using System;

namespace Adform.Bloom.Messages.Events
{
    public class AssignmentBaseEvent
    {
        public Guid SubjectId { get; set; }
        public Guid ActorId { get; set; }
        public Guid TenantId { get; set; }
        public string TenantName { get; set; }
        public int TenantLegacyId { get; set; }
        public string TenantType { get; set; }
        public Guid CorrelationId { get; set; }
        public long Timestamp { get; set; }
        public string[] Permissions { get; set; }
    }
}