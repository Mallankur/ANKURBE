using System;
using System.Collections.Generic;

namespace Adform.Bloom.Messages.Commands.AssetsReassignment
{
    public class ReassignUserAssetsCommand
    {
        public Guid CurrentUserId { get; set; }
        public IEnumerable<AssetsReassignment> AssetsReassignments { get; set; }
        public Guid MessageId { get; }
        public DateTime CreatedAt { get; }
        public Guid CorrelationId { get; set; }

        public ReassignUserAssetsCommand(Guid correlationId)
        {
            MessageId = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
            CorrelationId = correlationId;
        }
    }
}