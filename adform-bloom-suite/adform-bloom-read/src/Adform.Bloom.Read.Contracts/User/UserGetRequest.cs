using System;
using System.Runtime.Serialization;

namespace Adform.Bloom.Read.Contracts.User;

[DataContract]
public class UserGetRequest
{
    [DataMember(Order = 1)] public Guid Id { get; set; }
}