namespace Adform.Bloom.Domain.Entities
{
    public class Policy : NamedNode
    {
        public Policy(string policyName)
            : base(policyName)
        {
        }

        public Policy() : base(string.Empty)
        {
        }
    }
}