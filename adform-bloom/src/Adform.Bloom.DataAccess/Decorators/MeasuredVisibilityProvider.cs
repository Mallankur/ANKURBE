using Adform.Bloom.DataAccess.Interfaces;
using Adform.Ciam.Monitoring.Abstractions.CustomStructures;
using Adform.Ciam.Monitoring.Abstractions.Extensions;
using Adform.Ciam.SharedKernel.Entities;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Adform.Bloom.Infrastructure.Models;

namespace Adform.Bloom.DataAccess.Decorators
{
    public class MeasuredVisibilityProvider<TFilter, TDto> : IVisibilityProvider<TFilter, TDto>
        where TFilter : QueryParams
        where TDto : class
    {
        private static readonly string NodeType = typeof(TDto).Name;
        private readonly ICustomHistogram _histogram;
        private readonly IVisibilityProvider<TFilter, TDto> _inner;

        public MeasuredVisibilityProvider(IVisibilityProvider<TFilter, TDto> inner, ICustomHistogram histogram)
        {
            _inner = inner;
            _histogram = histogram;
        }
        
        public Task<bool> HasItemVisibilityAsync(ClaimsPrincipal subject, Guid resourceId, string? label = null)
        {
            return _histogram.MeasureAsync(() => _inner.HasItemVisibilityAsync(subject, resourceId, label), NodeType,
                "HasVisibilityAsync");
        }

        public Task<bool> HasVisibilityAsync(ClaimsPrincipal subject, TFilter filter, string? label = null)
        {
            return _histogram.MeasureAsync(() => _inner.HasVisibilityAsync(subject, filter, label),
            NodeType, "HasVisibilityAsync");
        }

        public Task<IEnumerable<Guid>> GetVisibleResourcesAsync(ClaimsPrincipal subject, TFilter filter, string? label = null)
        {
            return _histogram.MeasureAsync(() => _inner.GetVisibleResourcesAsync(subject, filter, label), 
                NodeType, "GetVisibleResourcesAsync");
        }

        public Task<EntityPagination<TDto>> EvaluateVisibilityAsync(ClaimsPrincipal subject,TFilter filter, int skip, int limit)
        {
            return _histogram.MeasureAsync(() => _inner.EvaluateVisibilityAsync(subject, filter, skip, limit),
                NodeType, "EvaluateVisibilityAsync");
        }
    }
}