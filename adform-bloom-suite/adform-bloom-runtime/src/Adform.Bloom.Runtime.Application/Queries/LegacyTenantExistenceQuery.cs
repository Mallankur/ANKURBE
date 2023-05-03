using FluentResults;
using MediatR;

namespace Adform.Bloom.Application.Queries
{
    public class LegacyTenantExistenceQuery : IRequest<Result<bool>>
    {
        public List<int> TenantLegacyIds { get; set; } = new();
        public string TenantType { get; set; } = "";
    }
}
