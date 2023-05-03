using Adform.Bloom.Infrastructure.Extensions;
using Xunit;

namespace Adform.Bloom.Unit.Test.Infrastructure.Extensions
{
    public class StringExtensionsTests
    {
        [Theory]
        [InlineData("String", "string")]
        [InlineData("string", "string")]
        [InlineData("STRING", "sTRING")]
        [InlineData(" string", " string")]
        [InlineData("/String", "/String")]
        [InlineData("", "")]
        public void String_ToLowerFirstCharacter_Sets_First_Character_To_Lower(string str, string expected)
        {
            Assert.Equal(expected, str.ToLowerFirstCharacter());
        }
    }
}