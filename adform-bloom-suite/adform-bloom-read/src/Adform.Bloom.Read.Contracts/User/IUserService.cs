using System.ServiceModel;
using System.Threading.Tasks;
using ProtoBuf.Grpc;

namespace Adform.Bloom.Read.Contracts.User;

[ServiceContract(Name = "UserService")]
public interface IUserService
{
    [OperationContract]
    Task<UserSearchResult> Find(UserSearchRequest request, CallContext context);
    [OperationContract]
    Task<UserGetResult> Get(UserGetRequest request, CallContext context);
}