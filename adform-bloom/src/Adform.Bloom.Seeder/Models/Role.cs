using System;

namespace Adform.Bloom.Seeder.Models
{
    public class Role : NamedNode
    {
        public Role(string roleName, string label, Guid? targetId, Guid? childId)
            : base(roleName, label, targetId, null, childId)
        {
        }

        public Role() : base(string.Empty, string.Empty, null, null, null)
        {
        }

        public override string TypeName => nameof(Role);
    }
}