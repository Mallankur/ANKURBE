using System;
using Adform.Bloom.Api.Graph.Common;
using Xunit;

namespace Adform.Bloom.Unit.Test.Api.Graph.Common
{
    public class GuidConverterTests
    {
        private readonly GuidConverter _converter = new();

        [Fact]
        public void Convert_ShouldReturnCorrectValue()
        {
            var guidObj = new Guid();
            var stringObj = guidObj.ToString();
            var success = _converter.TryCreateConverter(typeof(Guid), typeof(string), null, out var converter);

            Assert.True(success);
            Assert.NotNull(converter);
            Assert.Equal(stringObj, converter.Invoke(guidObj));
        }
    }
}