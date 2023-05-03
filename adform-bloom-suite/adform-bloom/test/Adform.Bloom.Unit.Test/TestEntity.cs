using Adform.Bloom.Domain.Entities;

namespace Adform.Bloom.Unit.Test
{
    public class TestEntity : NamedNode
    {
        public TestEntity() : base(string.Empty)
        {
        }

        public TestEntity(string nodeName) : base(nodeName)
        {
        }
    }
}