using Adform.Bloom.Api.Graph.Common;
using Xunit;

namespace Adform.Bloom.Unit.Test.Api.Graph.Common
{
    public class StringExtensionsTests
    {
        [Theory]
        [InlineData("demo","Demo")]
        [InlineData("twoFaAuthentication","TwoFaAuthentication")]
        [InlineData("businessAccount","BusinessAccount")]
        public void ToPascal_ShouldReturnCorrectValue(string input, string output)
        {
            var result = input.ToPascalCase();

            Assert.NotNull(result);
            Assert.Equal(output,result);
        }
        
        [Theory]
        [InlineData("Demo", "demo")]
        [InlineData("TwoFaAuthentication", "twoFaAuthentication")]
        [InlineData("BusinessAccount","businessAccount")]
        public void ToCamel_ShouldReturnCorrectValue(string input, string output)
        {
            var result = input.ToCamelCase();

            Assert.NotNull(result);
            Assert.Equal(output,result);
        }
    }
}