using FluentResults;
using MediatR;

namespace Adform.Bloom.Application.Queries
{
    public class RoleExistenceQuery : IRequest<Result<bool>>
    {
        public string RoleName { get; set; } = "";
        public Guid TenantId { get; set; } = Guid.Empty;
    }
}
