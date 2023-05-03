using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Adform.Bloom.Domain.Ports
{
    public interface ITenantValidator
    {
        Task<bool> DoesTenantExist(Guid tenantId);
        Task<bool> DoTenantsExist(IReadOnlyCollection<Guid> tenantIds);
    }
}
