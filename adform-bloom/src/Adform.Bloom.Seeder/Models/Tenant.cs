using System;

namespace Adform.Bloom.Seeder.Models
{
    public class Tenant : NamedNode
    {
        public Tenant(string name, string label, Guid? targetId)
            : base(name, label, targetId, null, null)
        {
        }

        public Tenant() : base(string.Empty, string.Empty, null, null, null)
        {
        }

        public override string TypeName => nameof(Tenant);
    }
}