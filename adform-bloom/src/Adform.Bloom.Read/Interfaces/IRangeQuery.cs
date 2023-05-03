using MediatR;
using System.Security.Claims;

namespace Adform.Bloom.Read.Interfaces
{
    public interface IRangeQuery<TFilter, TEntity> : IRequest<TEntity>
    {
        ClaimsPrincipal Principal { get; }
        int Offset { get; }
        int Limit { get; }
        TFilter Filter { get; }
    }
}