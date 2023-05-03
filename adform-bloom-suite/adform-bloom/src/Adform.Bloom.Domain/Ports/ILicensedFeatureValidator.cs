using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Adform.Bloom.Domain.Ports
{
    public interface ILicensedFeatureValidator
    {
        Task<bool> DoLicensedFeaturesExist(IReadOnlyCollection<Guid> licensedFeatureIds);
    }
}