using MediatR;
using System;
using System.Security.Claims;

namespace Adform.Bloom.Read.Interfaces
{
    public interface ISingleQuery<TFilter, TEntity> : IRequest<TEntity>
    {
        Guid Id { get; }
        TFilter Filter { get; }
        ClaimsPrincipal Principal { get; }
    }
}