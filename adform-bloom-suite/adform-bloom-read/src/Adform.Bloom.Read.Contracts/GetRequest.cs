using System;
using System.Runtime.Serialization;

namespace Adform.Bloom.Read.Contracts;

[DataContract]
public class GetRequest
{
    [DataMember(Order = 1)] public Guid Id { get; set; }
}