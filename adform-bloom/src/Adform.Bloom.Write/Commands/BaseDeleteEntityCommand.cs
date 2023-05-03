using MediatR;
using System;
using System.Security.Claims;

namespace Adform.Bloom.Write.Commands
{
    public abstract class BaseDeleteEntityCommand : IRequest
    {
        protected BaseDeleteEntityCommand(ClaimsPrincipal principal, Guid idOfEntityToDeleted)
        {
            IdOfEntityToDeleted = idOfEntityToDeleted;
            Principal = principal;
        }

        public ClaimsPrincipal Principal { get; }
        public Guid IdOfEntityToDeleted { get; }
    }
}