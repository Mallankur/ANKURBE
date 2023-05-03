using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Adform.Bloom.Read.Contracts.User;

[DataContract]
public class UserSearchRequest
{
    [DataMember(Order = 1)] public int Offset { get; set; }
    [DataMember(Order = 2)] public int Limit { get; set; } = 100;
    [DataMember(Order = 3)] public string OrderBy { get; set; } = "Id";
    [DataMember(Order = 4)] public SortingOrder SortingOrder { get; set; } = SortingOrder.Ascending;
    [DataMember(Order = 5)] public string? Search { get; set; }
    [DataMember(Order = 6)] public ICollection<Guid>? Ids { get; set; }
    [DataMember(Order = 7)] public UserType? Type { get; set; }

}