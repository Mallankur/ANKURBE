using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Adform.Ciam.SharedKernel.Entities;

namespace Adform.Bloom.DataAccess.Interfaces
{
    public interface IAccessProvider<TContextDto, TFilterInput, TOutputDto>
        where TContextDto : class
        where TOutputDto : class
        where TFilterInput : class
    {
        Task<EntityPagination<TOutputDto>> EvaluateAccessAsync(ClaimsPrincipal subject, TContextDto context, int skip,
            int limit, TFilterInput filter, CancellationToken cancellationToken = default);
    }
}