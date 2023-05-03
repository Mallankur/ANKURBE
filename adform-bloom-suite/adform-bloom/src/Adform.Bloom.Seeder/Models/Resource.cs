using System;

namespace Adform.Bloom.Seeder.Models
{
    public class Resource : NamedNode
    {
        public Resource(string resourceName, string label, Guid? targetId)
            : base(resourceName, label, targetId, null, null)
        {
        }

        public Resource() : base(string.Empty, string.Empty, null, null, null)
        {
        }

        public override string TypeName => nameof(Resource);
    }
}