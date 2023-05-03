using System;
using System.Runtime.Serialization;

namespace Adform.Bloom.Read.Contracts.BusinessAccount;

[DataContract]
public class BusinessAccountResult
{
    [DataMember(Order = 1)] public Guid Id { get; set; }
    [DataMember(Order = 2)] public int LegacyId { get; set; }
    [DataMember(Order = 3)] public string Name { get; set; } = string.Empty;
    [DataMember(Order = 5)] public BusinessAccountType Type { get; set; }
    [DataMember(Order = 6)] public BusinessAccountStatus Status { get; set; }
}

public enum BusinessAccountStatus
{
    Active,
    Inactive,
    Pending
}