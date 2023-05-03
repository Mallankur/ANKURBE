using System;
using System.Security.Claims;

namespace Adform.Bloom.Write.Commands
{
    public class DeleteFeatureCommand : BaseDeleteEntityCommand
    {
        public DeleteFeatureCommand(ClaimsPrincipal principal, Guid featureId)
            : base(principal, featureId)
        {
        }
    }
}