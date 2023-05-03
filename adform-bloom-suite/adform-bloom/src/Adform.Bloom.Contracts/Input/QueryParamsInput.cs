using System;
using System.Collections.Generic;

namespace Adform.Bloom.Contracts.Input
{
    public class QueryParamsInput : SortingParamsInput
    {
        public Guid? ContextId { get; set; }
        public IReadOnlyCollection<Guid>? ResourceIds { get; set; }
        public string? Search { get; set; }
    }
}