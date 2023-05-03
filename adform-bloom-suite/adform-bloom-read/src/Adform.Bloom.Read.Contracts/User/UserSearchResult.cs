using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Adform.Bloom.Read.Contracts.User;

[DataContract]
public class UserSearchResult
{
    [DataMember(Order = 1)]
    public int Offset { get; set; }
    [DataMember(Order = 2)]
    public int Limit { get; set; }
    [DataMember(Order = 3)]
    public int TotalItems { get; set; }
    [DataMember(Order = 4)]
    public ICollection<UserResult> Users { get; set; } = new List<UserResult>();
}