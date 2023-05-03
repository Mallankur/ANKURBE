using System;

namespace Adform.Bloom.Seeder.Models
{
    public class Subject : NamedNode
    {
        public Subject(string name, string label, Guid? targetId)
            : base(name, label, targetId, null, null)
        {
        }

        public Subject() : base(string.Empty, string.Empty, null, null, null)
        {
        }

        public override string TypeName => nameof(Subject);
    }
}