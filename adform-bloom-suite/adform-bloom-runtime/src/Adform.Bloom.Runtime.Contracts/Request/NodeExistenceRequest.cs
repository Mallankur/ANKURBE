using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Adform.Bloom.Client.Contracts.Request
{
    [DataContract]
    public class NodeExistenceRequest
    {
        [DataMember(Order = 1)] public List<NodeDescriptor> NodeDescriptors { get; set; } = new List<NodeDescriptor>();
    }
}