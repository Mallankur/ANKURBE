using Adform.Bloom.Write.Commands;
using System;
using System.Security.Claims;

namespace Adform.Bloom.Unit.Test.Write
{
    public class TestDeleteCommand : BaseDeleteEntityCommand
    {
        public TestDeleteCommand(ClaimsPrincipal principal, Guid id)
            : base(principal, id)
        {
        }
    }
}