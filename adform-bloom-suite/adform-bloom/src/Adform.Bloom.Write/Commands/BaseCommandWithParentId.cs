using System;
using System.Security.Claims;

namespace Adform.Bloom.Write.Commands
{
    public abstract class BaseCommandWithParentId<TEntity> : NamedCreateCommand<TEntity>
    {
        protected BaseCommandWithParentId(
            ClaimsPrincipal principal, 
            Guid? parentId,
            string name,
            string description = "", 
            bool isEnabled = true)
            : base(principal, name, description, isEnabled)
        {
            ParentId = parentId;
        }

        public Guid? ParentId { get; }
    }
}