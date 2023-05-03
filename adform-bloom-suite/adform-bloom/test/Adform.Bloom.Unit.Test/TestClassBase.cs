using AutoFixture;

namespace Adform.Bloom.Unit.Test
{
    public class TestClassBase
    {
        protected Fixture Fixture { get; }

        public TestClassBase()
        {
            Fixture = new Fixture();
        }
    }
}
