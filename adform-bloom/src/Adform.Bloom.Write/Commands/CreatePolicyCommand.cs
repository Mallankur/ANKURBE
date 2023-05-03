using System;
using System.Security.Claims;
using Adform.Bloom.Domain.Entities;

namespace Adform.Bloom.Write.Commands
{
    public class CreatePolicyCommand : BaseCommandWithParentId<Policy>
    {
        public CreatePolicyCommand(ClaimsPrincipal principal, 
            Guid? parentId, string name, string description = "", bool isEnabled = true)
            : base(principal, parentId, name, description, isEnabled)
        {
        }
    }
}