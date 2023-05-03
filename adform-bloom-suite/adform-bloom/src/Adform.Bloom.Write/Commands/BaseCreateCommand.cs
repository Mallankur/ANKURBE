using System;
using MediatR;
using System.Security.Claims;

namespace Adform.Bloom.Write.Commands
{
    public abstract class BaseCreateCommand<TEntity> : IRequest<TEntity>
    {

        protected BaseCreateCommand(ClaimsPrincipal principal, bool isEnabled = true)
        {
            Principal = principal;
            IsEnabled = isEnabled;
        }

        public ClaimsPrincipal Principal { get; }

        public bool IsEnabled { get; set; }

        public long CreatedAt { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        public long UpdatedAt { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
}