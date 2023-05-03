using System.IO;
using Adform.Bloom.Read.Infrastructure.Configuration;
using Adform.Bloom.Read.Infrastructure.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Xunit;

namespace Adform.Bloom.Read.Unit.Test.Infrastructure;

public class ConfigurationTests
{
    private const string ConfigurationCorrect = "Correct";
    private const string ConfigurationMissingHosts = "MissingHosts";
    private const string PortIsNegative = "PortIsNegative";
    private readonly IConfigurationRoot _configurationRoot;
    public ConfigurationTests()
    {
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddJsonFile(
            Path.Combine(Directory.GetCurrentDirectory(), "testservicecollection.json"), false);
        _configurationRoot = configurationBuilder.Build();
    }

    [Fact]
    public void ValidateDefaultNpgSqlConfiguration_HostsIsMissing_ExceptionIsThrown()
    {
        //Arrange
        var root = GetSubSection(ConfigurationMissingHosts);
        var configuration = root.GetSection(Paths.Configuration).Get<NpgSqlConfiguration>();
        //Act & Assert
        var ex = Assert.Throws<OptionsValidationException>(() => configuration.Validate());
        Assert.Equal(nameof(NpgSqlConfiguration.Host), ex.OptionsName);
        Assert.Equal(typeof(NpgSqlConfiguration), ex.OptionsType);
    }

    [Fact]
    public void ValidateDefaultNpgSqlConfiguration_ItIsCorrect_NoExceptionIsThrown()
    {
        //Arrange
        var root = GetSubSection(ConfigurationCorrect);
        var configuration = root.GetSection(Paths.Configuration).Get<NpgSqlConfiguration>();
        //Act
        configuration.Validate();
        //Assert
        Assert.NotNull(configuration);
        Assert.Equal("localhost", configuration.Host);
        Assert.Equal("default", configuration.Database);
        Assert.Equal(5432, configuration.Port);
    }

    [Fact]
    public void ValidateDefaultNpgSqlConfiguration_PortIsNegative_DefaultSettingsAreUsed()
    {
        //Arrange
        var root = GetSubSection(PortIsNegative);
        var configuration = root.GetSection(Paths.Configuration).Get<NpgSqlConfiguration>();
        //Act & Assert
        var ex = Assert.Throws<OptionsValidationException>(() => configuration.Validate());
        Assert.Equal(nameof(NpgSqlConfiguration.Port), ex.OptionsName);
        Assert.Equal(typeof(NpgSqlConfiguration), ex.OptionsType);
    }


    [Fact]
    public void ValidateDefaultNpgSqlConfiguration_PropertiesAreMissing_DefaultSettingsAreUsed()
    {
        //Arrange
        var root = GetSubSection(ConfigurationCorrect);
        var configuration = root.GetSection(Paths.Configuration).Get<NpgSqlConfiguration>();
        //Act & Assert
        //Assert
        Assert.NotNull(configuration);
        Assert.Equal("localhost", configuration.Host);
        Assert.Equal("default", configuration.Database);
        Assert.Equal(5432, configuration.Port);
    }
    private IConfigurationSection GetSubSection(string path)
    {
        return _configurationRoot.GetSection(path);
    }

}