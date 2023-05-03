using System;
using System.Security.Claims;
using Adform.Bloom.Read.Interfaces;

namespace Adform.Bloom.Read.Queries
{
    public abstract class BaseSingleQuery<TFilter, TEntity> : ISingleQuery<TFilter, TEntity>
    {
        public Guid Id { get; }
        public ClaimsPrincipal Principal { get; }
        public TFilter Filter { get; }

        protected BaseSingleQuery(ClaimsPrincipal principal, Guid id, TFilter filter)
        {
            Id = id;
            Filter = filter;
            Principal = principal;
        }
    }
}