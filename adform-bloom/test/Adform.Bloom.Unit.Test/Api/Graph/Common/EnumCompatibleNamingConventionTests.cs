using System;
using Adform.Bloom.Api.Graph.Common;
using Adform.Bloom.Contracts.Input;
using Xunit;

namespace Adform.Bloom.Unit.Test.Api.Graph.Common
{
    public class EnumCompatibleNamingConventionTests
    {
        private readonly EnumCompatibleNamingConvention _convention = new();

        [Fact]
        public void Should_Return_Correct_Name_Format_For_Enum_Value()
        {
            var newValue = _convention.GetEnumValueName(LinkOperation.Assign);
            Assert.Equal(LinkOperation.Assign.ToString().ToLowerInvariant(), newValue);
        }

        [Fact]
        public void Should_Throw_Error_If_Value_Null()
        {
            Assert.Throws<ArgumentNullException>(() => _convention.GetEnumValueName(null));
        }
    }
}