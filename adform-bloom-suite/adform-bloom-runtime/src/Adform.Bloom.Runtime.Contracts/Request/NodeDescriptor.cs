using System;
using System.Runtime.Serialization;

namespace Adform.Bloom.Client.Contracts.Request
{
    [DataContract]
    public class NodeDescriptor
    {
        [DataMember(Order = 1)] public string Label { get; set; } = string.Empty;
        [DataMember(Order = 2)] public Guid? Id { get; set; }
        [DataMember(Order = 3)] public string? UniqueName { get; set; }
    }
}