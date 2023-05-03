using System;

namespace Adform.Bloom.Seeder.Models
{
    public class Permission : NamedNode
    {
        public Permission(string permissionName, string label, Guid? targetId, Guid? childId) : base(permissionName,
            label, targetId, null, childId)
        {
        }

        public Permission() : base(string.Empty, string.Empty, null, null, null)
        {
        }

        public override string TypeName => nameof(Permission);
    }
}