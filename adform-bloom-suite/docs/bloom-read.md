# Bloom Read

This service is responsible of all the queries of the read model. Is exposed throught GRPC.

## Query

### Contract User

```proto
syntax = "proto3";
package Adform.Bloom.Read.Contracts.User;
import "protobuf-net/bcl.proto"; // schema for protobuf-net's handling of core .NET types

enum SortingOrder {
   Ascending = 0;
   Descending = 1;
}
message UserGetRequest {
   .bcl.Guid Id = 1; // default value could not be applied: 00000000-0000-0000-0000-000000000000
}
message UserGetResult {
   UserResult User = 1;
}
message UserResult {
   .bcl.Guid Id = 1; // default value could not be applied: 00000000-0000-0000-0000-000000000000
   string Username = 2;
   string Name = 3;
   string Email = 4;
   string Phone = 5;
   string Timezone = 6;
   string Locale = 7;
   string FirstName = 8;
   string LastName = 9;
   string Company = 10;
   string Title = 11;
   bool TwoFaEnabled = 12;
   bool SecurityNotifications = 13;
   UserStatus Status = 14;
   UserType Type = 15;
}
message UserSearchRequest {
   int32 Offset = 1;
   int32 Limit = 2;
   string OrderBy = 3;
   SortingOrder SortingOrder = 4;
   string Search = 5;
   repeated .bcl.Guid Ids = 6;
   UserType Type = 7;
}
message UserSearchResult {
   int32 Offset = 1;
   int32 Limit = 2;
   int32 TotalItems = 3;
   repeated UserResult Users = 4;
}
enum UserStatus {
   Unknown = 0;
   Active = 1;
   Disabled = 2;
   Locked = 3;
   PendingRegistration = 4;
   ResetPassword = 5;
}
enum UserType {
   MasterAccount = 0;
   Trafficker = 1;
}
service UserService {
   rpc Find (UserSearchRequest) returns (UserSearchResult);
   rpc Get (UserGetRequest) returns (UserGetResult);
}
```
### Contract BusinessAccount

```proto
syntax = "proto3";
package Adform.Bloom.Read.Contracts.BusinessAccount;
import "protobuf-net/bcl.proto"; // schema for protobuf-net's handling of core .NET types

message BusinessAccountGetResult {
   BusinessAccountResult BusinessAccount = 1;
}
message BusinessAccountResult {
   .bcl.Guid Id = 1; // default value could not be applied: 00000000-0000-0000-0000-000000000000
   int32 LegacyId = 2;
   string Name = 3;
   BusinessAccountType Type = 5;
   BusinessAccountStatus Status = 6;
}
message BusinessAccountSearchRequest {
   int32 Offset = 1;
   int32 Limit = 2;
   string OrderBy = 3;
   SortingOrder SortingOrder = 4;
   string Search = 5;
   repeated .bcl.Guid Ids = 6;
   BusinessAccountType Type = 7;
}
message BusinessAccountSearchResult {
   int32 Offset = 1;
   int32 Limit = 2;
   int32 TotalItems = 3;
   repeated BusinessAccountResult BusinessAccounts = 4;
}
enum BusinessAccountStatus {
   Active = 0;
   Inactive = 1;
   Pending = 2;
}
enum BusinessAccountType {
   Adform = 0;
   Agency = 1;
   Publisher = 2;
   DataProvider = 3;
}
message GetRequest {
   .bcl.Guid Id = 1; // default value could not be applied: 00000000-0000-0000-0000-000000000000
}
enum SortingOrder {
   Ascending = 0;
   Descending = 1;
}
service BusinessAccountService {
   rpc FindBusinessAccounts (BusinessAccountSearchRequest) returns (BusinessAccountSearchResult);
   rpc GetBusinessAccount (GetRequest) returns (BusinessAccountGetResult);
}
```