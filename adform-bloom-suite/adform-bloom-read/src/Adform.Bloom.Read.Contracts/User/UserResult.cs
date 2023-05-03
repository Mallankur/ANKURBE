using System;
using System.Runtime.Serialization;

namespace Adform.Bloom.Read.Contracts.User;

[DataContract]
public class UserResult
{
    [DataMember(Order = 1)] public Guid Id { get; set; }
    [DataMember(Order = 2)] public string Username { get; set; } = string.Empty;
    [DataMember(Order = 3)] public string Name { get; set; } = string.Empty;
    [DataMember(Order = 4)] public string Email { get; set; } = string.Empty;
    [DataMember(Order = 5)] public string Phone { get; set; } = string.Empty;
    [DataMember(Order = 6)] public string Timezone { get; set; } = string.Empty;
    [DataMember(Order = 7)] public string Locale { get; set; } = string.Empty;
    [DataMember(Order = 8)] public string FirstName { get; set; } = string.Empty;
    [DataMember(Order = 9)] public string LastName { get; set; } = string.Empty;
    [DataMember(Order = 10)] public string Company { get; set; } = string.Empty;
    [DataMember(Order = 11)] public string? Title { get; set; }
    [DataMember(Order = 12)] public bool TwoFaEnabled { get; set; }
    [DataMember(Order = 13)] public bool SecurityNotifications { get; set; }
    [DataMember(Order = 14)] public UserStatus Status { get; set; }
    [DataMember(Order = 15)] public UserType Type { get; set; }

}