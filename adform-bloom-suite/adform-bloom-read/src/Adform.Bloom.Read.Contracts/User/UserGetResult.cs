using System.Runtime.Serialization;

namespace Adform.Bloom.Read.Contracts.User;

[DataContract]
public class UserGetResult
{
    [DataMember(Order = 1)]
    public UserResult? User { get; set; }
}