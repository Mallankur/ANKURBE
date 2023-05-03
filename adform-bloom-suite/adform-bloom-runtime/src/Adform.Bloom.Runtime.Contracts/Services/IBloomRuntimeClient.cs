using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Client.Contracts.Request;
using Adform.Bloom.Client.Contracts.Response;

namespace Adform.Bloom.Client.Contracts.Services
{
    [ServiceContract(Name = "BloomRuntimeService")]
    public interface IBloomRuntimeClient
    {
        [OperationContract]
        Task<IEnumerable<RuntimeResponse>> InvokeAsync(SubjectRuntimeRequest data,
            CancellationToken cancellationToken = default);
        
        [OperationContract]
        Task<IEnumerable<RuntimeResponse>> IntersectionAsync(SubjectIntersectionRequest data,
            CancellationToken cancellationToken = default);

        [OperationContract]
        Task<ExistenceResponse> LegacyTenantExistenceAsync(LegacyTenantExistenceRequest request,
            CancellationToken cancellationToken = default);

        [OperationContract]
        Task<ExistenceResponse> RoleExistenceAsync(RoleExistenceRequest request,
            CancellationToken cancellationToken = default);

        [OperationContract]
        Task<ExistenceResponse> NodeExistenceAsync(NodeExistenceRequest request,
            CancellationToken cancellationToken = default);

        [OperationContract]
        Task<bool> IsHealthy();
    }
}