using System;
using System.Threading.Tasks;

namespace Adform.Bloom.Domain.Ports
{
    public interface IPolicyValidator
    {
        Task<bool> DoesPolicyExist(Guid policyId);
    }
}
