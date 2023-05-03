using Adform.Bloom.Write.Commands;
using System;
using System.Security.Claims;

namespace Adform.Bloom.Unit.Test.Write
{
    public class TestCreateCommand : BaseCommandWithParentId<TestEntity>
    {
        public TestCreateCommand(ClaimsPrincipal principal,
            string name, string description, bool isEnabled, Guid? parentId = null) : base(principal,
            parentId, name, description, isEnabled)
        {
        }
    }
}