using System;
using System.Collections.Generic;
using Adform.Ciam.OngDb.Repository;

namespace Adform.Bloom.Infrastructure.Models
{
    public class QueryParams : SortingParams
    {
        public Guid? ContextId { get;  set; }
        public IReadOnlyCollection<Guid>? ResourceIds { get; set; }
        public string? Search { get; set; }
    }
}