using Adform.Bloom.Infrastructure.Extensions;
using System;
using Xunit;

namespace Adform.Bloom.Unit.Test.Infrastructure.Extensions
{
    public class GuidExtensionsTests
    {
        [Fact]
        public void Guid_Combine_Produces_Same_Result_For_The_Same_Guids()
        {
            var guid1 = Guid.NewGuid();
            var guid2 = Guid.NewGuid();
            var combined = guid1.Combine(guid2);

            for (var i = 0; i < 10; i++)
            {
                var combinedAgain = guid1.Combine(guid2);
                Assert.Equal(combined, combinedAgain);
            }
        }

        [Fact]
        public void Guid_Combine_Produces_Same_Result_Interchangably()
        {
            var guid1 = Guid.NewGuid();
            var guid2 = Guid.NewGuid();
            var combined = guid1.Combine(guid2);
            var combinedOpposite = guid2.Combine(guid1);

            Assert.Equal(combined, combinedOpposite);
        }
    }
}