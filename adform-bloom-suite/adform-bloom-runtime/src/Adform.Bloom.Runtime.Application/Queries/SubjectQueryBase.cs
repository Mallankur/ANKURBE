using System.Text.Json;
using Adform.Bloom.Runtime.Read.Entities;
using MediatR;

namespace Adform.Bloom.Application.Queries
{
    public class SubjectQueryBase : IRequest<IEnumerable<RuntimeResult>>
    {
        public Guid SubjectId { get; set; } = Guid.Empty;
        public IEnumerable<Guid> TenantIds { get; set; } = Enumerable.Empty<Guid>();
        public IEnumerable<int> TenantLegacyIds { get; set; } = Enumerable.Empty<int>();
        public string? TenantType { get; set; }
        public IEnumerable<string> PolicyNames { get; set; } = Enumerable.Empty<string>();
        public bool InheritanceEnabled { get; set; } = true;

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}