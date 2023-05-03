using System;

namespace Adform.Bloom.Domain.Entities
{
    public class ConnectedEntity<T> where T: BaseNode
    {
        public Guid StartNodeId { get; set; }
        public T? ConnectedNode { get; set; }
    };
}