using System;

namespace Adform.Bloom.Seeder.Models
{
    public class Group : NamedNode
    {
        public Group(string name, string label, Guid? targetId, Guid? secondTargetId) : base(name, label, targetId,
            secondTargetId, null)
        {
        }

        public Group() : base(string.Empty, string.Empty, null, null, null)
        {
        }

        public override string TypeName => nameof(Group);
    }
}