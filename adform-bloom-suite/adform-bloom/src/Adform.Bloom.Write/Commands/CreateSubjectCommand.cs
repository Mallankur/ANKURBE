using System;
using System.Collections.Generic;
using System.Security.Claims;
using Adform.Bloom.Domain.Entities;

namespace Adform.Bloom.Write.Commands
{
#warning Used as orchestration.
    public class CreateSubjectCommand : BaseCreateCommand<Subject>
    {
        public Guid Id { get; }
        public string Email { get; set; }
        public IReadOnlyCollection<RoleTenant>? RoleTenantIds { get; }

        public CreateSubjectCommand(ClaimsPrincipal principal, Guid id, string email, bool isEnabled, 
            IReadOnlyCollection<RoleTenant>? roleTenantIds = null)
            : base(principal, isEnabled)
        {
            Id = id;
            Email = email;
            RoleTenantIds = roleTenantIds;
        }
    }
}