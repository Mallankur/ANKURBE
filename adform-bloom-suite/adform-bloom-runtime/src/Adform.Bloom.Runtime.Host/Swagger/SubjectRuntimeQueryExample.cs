using System;
using System.Collections.Generic;
using Adform.Bloom.Application.Queries;
using Swashbuckle.AspNetCore.Filters;

namespace Adform.Bloom.Runtime.Host.Swagger
{
    public class SubjectRuntimeQueryExample : IExamplesProvider<SubjectRuntimeQuery>
    {
        public SubjectRuntimeQuery GetExamples() => new SubjectRuntimeQuery
        {
            SubjectId = Guid.Empty,
            PolicyNames = new List<string>() { string.Empty },
            TenantIds = new List<Guid>() { Guid.Empty },
            InheritanceEnabled = true
        };
    }
}