using System.Runtime.Serialization;

namespace Adform.Bloom.Read.Contracts.BusinessAccount;

[DataContract]
public class BusinessAccountGetResult
{
    [DataMember(Order = 1)]
    public BusinessAccountResult? BusinessAccount { get; set; }
}