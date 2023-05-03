using System.Data;
using Adform.Bloom.Read.Infrastructure.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Adform.Bloom.Read.Unit.Test.Infrastructure;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void ConfigurationNpgSql_IDbConnectionShouldBeRegisteredInDIContainer()
    {
        //Arrange
        var serviceCollection = new ServiceCollection();
        //Act
        serviceCollection.ConfigurationNpgSql(o =>
        {
            o.Port = 3000;
        });
        var prv = serviceCollection.BuildServiceProvider();
        var dbConnection = prv.GetRequiredService<IDbConnection>();
        //Assert
        Assert.NotNull(dbConnection);
        Assert.Contains(typeof(IDbConnection), dbConnection.GetType().GetInterfaces());
    }

}