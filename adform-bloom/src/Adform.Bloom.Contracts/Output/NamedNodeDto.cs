namespace Adform.Bloom.Contracts.Output
{
    public class NamedNodeDto: BaseNodeDto
    {
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    }
}