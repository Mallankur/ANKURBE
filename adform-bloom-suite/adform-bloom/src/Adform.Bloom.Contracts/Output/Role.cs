namespace Adform.Bloom.Contracts.Output
{
    public class Role : NamedNodeDto
    {
        public RoleCategory Type { get; set; } = RoleCategory.Template;
    }

    public enum RoleCategory
    {
        Template,
        Custom,
        Transitional,
    }
}