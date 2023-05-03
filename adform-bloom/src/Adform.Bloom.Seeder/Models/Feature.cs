using System;

namespace Adform.Bloom.Seeder.Models
{
    public class Feature : NamedNode
    {
        public Feature(string nodeName, string label, Guid? targetId, Guid? childId) :
            base(nodeName, label, targetId, null, childId)
        {
        }

        public override string TypeName => nameof(Feature);
    }
}