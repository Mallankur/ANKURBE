using Adform.Bloom.Read.Domain.Entities;
using Adform.Bloom.Read.Infrastructure.Extensions;
using Xunit;

namespace Adform.Bloom.Read.Unit.Test.Infrastructure;

public class NpgSqlConfigurationExtensionsTests
{
    [Theory]
    [InlineData("Id", "Id")]
    [InlineData("NotExistant", "Id")]
    [InlineData("Username", "Username")]
    [InlineData("FirstName", "FirstName")]
    [InlineData("LastName", "LastName")]
    public void ToUnderscoreCase_Returns_Expected_Result(string input, string output)
    {
        // Act
        var result = input.ToPropertyOrDefault<User>();
        // Assert
        Assert.Equal(output, result);
    }
}