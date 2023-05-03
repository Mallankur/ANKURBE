using System;
using System.Security.Claims;

namespace Adform.Bloom.Write.Commands
{
    public class DeletePolicyCommand : BaseDeleteEntityCommand
    {
        public DeletePolicyCommand(ClaimsPrincipal principal, Guid policyId)
            : base(principal, policyId)
        {
        }
    }
}