using System.ServiceModel;
using System.Threading.Tasks;
using ProtoBuf.Grpc;

namespace Adform.Bloom.Read.Contracts.BusinessAccount;

[ServiceContract(Name = "BusinessAccountService")]
public interface IBusinessAccountService
{
    [OperationContract]
    Task<BusinessAccountGetResult> GetBusinessAccount(GetRequest request,
        CallContext context);

    [OperationContract]
    Task<BusinessAccountSearchResult> FindBusinessAccounts(BusinessAccountSearchRequest request, CallContext context);
}