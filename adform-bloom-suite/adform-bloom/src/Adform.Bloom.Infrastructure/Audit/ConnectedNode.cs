using System;
using System.Text.Json;

namespace Adform.Bloom.Infrastructure.Audit
{
    public class ConnectedNode
    {
        public ConnectedNode(Guid originId, Guid targetId, string? relationType)
        {
            OriginId = originId;
            TargetId = targetId;
            TargetParentId = null;
            RelationType = relationType;
        }
        public ConnectedNode(Guid originId, Guid targetParentId,Guid targetId, string? relationType)
        {
            OriginId = originId;
            TargetId = targetId;
            TargetParentId = targetParentId;
            RelationType = relationType;
        }

        public Guid? TargetParentId { get; }
        public Guid TargetId { get; }
        public Guid OriginId { get; }
        public string? RelationType { get; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}