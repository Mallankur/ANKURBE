namespace Adform.Bloom.Domain.Entities
{
    public class Role : NamedNode
    {
        public Role(string roleName)
            : base(roleName)
        {
        }

        public Role() : base(string.Empty)
        {
        }
    }
}