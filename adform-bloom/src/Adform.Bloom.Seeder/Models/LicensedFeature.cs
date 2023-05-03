using System;

namespace Adform.Bloom.Seeder.Models
{
    public class LicensedFeature : NamedNode
    {
        public LicensedFeature(string nodeName, string label, Guid? targetId, Guid? childId) :
            base(nodeName, label, targetId, null, childId)
        {
        }

        public override string TypeName => nameof(LicensedFeature);
    }
}