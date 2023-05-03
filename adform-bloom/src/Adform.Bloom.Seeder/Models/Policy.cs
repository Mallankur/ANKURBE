using System;

namespace Adform.Bloom.Seeder.Models
{
    public class Policy : NamedNode
    {
        public Policy(string nodeName, string label, Guid? targetId)
            : base(nodeName, label, targetId, null, null)
        {
        }

        public Policy() : base(string.Empty, string.Empty, null, null, null)
        {
        }

        public override string TypeName => nameof(Policy);
    }
}