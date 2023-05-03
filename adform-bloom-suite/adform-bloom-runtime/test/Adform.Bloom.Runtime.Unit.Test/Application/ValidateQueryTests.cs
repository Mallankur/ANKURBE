using System;
using System.Threading.Tasks;
using Adform.Bloom.Application.Queries;
using Adform.Bloom.Application.Validators;
using Adform.Ciam.ExceptionHandling.Abstractions.Extensions;
using Xunit;

namespace Adform.Bloom.Runtime.Read.Test.Application
{
    public class ValidateQueryTests
    {
        [Theory]
        [MemberData(nameof(Common.Validation), MemberType = typeof(Common))]
        public async Task Validate_ThrowsIf_Invalid_Input(SubjectRuntimeQuery data, Exception? exception)
        {
            //Arrange
            var validateQuery = new ValidateQuery();

            //Act & Assert
            var exceptionResult = Record.Exception(() => validateQuery.Validate(data));
            if (exception != null)
            {
                Assert.Equal(exceptionResult.GetReason(), exception.GetReason());
                Assert.Equal(exceptionResult.Message, exception.Message);
            }
            else
            {
                Assert.Null(exceptionResult);
            }
        }
    }
}