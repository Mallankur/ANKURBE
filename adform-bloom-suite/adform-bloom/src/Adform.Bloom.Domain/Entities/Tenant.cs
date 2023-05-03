namespace Adform.Bloom.Domain.Entities
{
    public class Tenant : NamedNode
    {
        public int LegacyId { get; set; } = 0;
        public Tenant(string name)
            : base(name)
        {
        }

        public Tenant() : base(string.Empty)
        {
        }
    }
}