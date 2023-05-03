namespace Adform.Bloom.Domain.Entities
{
    public class Permission : NamedNode
    {
        public Permission(string permissionName) : base(permissionName)
        {
        }

        public Permission() : base(string.Empty)
        {
        }
    }
}