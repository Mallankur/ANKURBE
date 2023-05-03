using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Adform.Bloom.Infrastructure.Models;
using Adform.Ciam.SharedKernel.Entities;
using Mapster;

namespace Adform.Bloom.DataAccess.Interfaces
{
    public interface IVisibilityProvider<TFilter, TDto>
        where TFilter : QueryParams
        where TDto : class
    { 
        Task<bool> HasItemVisibilityAsync(ClaimsPrincipal subject, Guid resourceId, string? label = null)
        {
            var filter = new 
            {
                ResourceIds = new List<Guid> {resourceId}
            };
            return  HasVisibilityAsync(subject, filter.Adapt<TFilter>(), label);
        }
           

        Task<bool> HasVisibilityAsync(ClaimsPrincipal subject, TFilter filter, string? label = null);

        Task<IEnumerable<Guid>> GetVisibleResourcesAsync(ClaimsPrincipal subject, TFilter filter, string? label = null);

        Task<EntityPagination<TDto>> EvaluateVisibilityAsync(ClaimsPrincipal subject, TFilter filter, int skip,
            int limit);
    }
}