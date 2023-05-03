using System;
using System.Collections.Generic;

namespace Adform.Bloom.Infrastructure.Models
{
    public class QueryParamsRoles : QueryParams
    {
        public IReadOnlyCollection<Guid>? TenantIds { get; set; }
        public bool? PrioritizeTemplateRoles { get; set; }
    }
}